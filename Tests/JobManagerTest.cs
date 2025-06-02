using NUnit.Framework;
using Moq;
using Core.Model;
using Core.Model.Implementations;
using Core.Model.Interfaces;
using Core.Utils;
using System.Collections.Generic;

namespace Tests
{
    public class JobManagerTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IConfigurationManager> _mockConfigManager;
        private Mock<IUIService> _mockUiService;
        private Mock<IResourceService> _mockResourceService;
        private JobManager _jobManager;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockUiService = new Mock<IUIService>();
            _mockResourceService = new Mock<IResourceService>();
            _mockConfigManager = new Mock<IConfigurationManager>();

            // By default, return empty list for jobs
            _mockConfigManager.Setup(x => x.LoadJobs()).Returns(new List<BackupJob>());

            _jobManager = new JobManager(
                _mockLogger.Object,
                _mockConfigManager.Object,
                _mockUiService.Object,
                _mockResourceService.Object
            );
        }

        [Test]
        public void AddBackupJob_ShouldAddJob_WhenJobDoesNotExist()
        {
            var job = new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full };

            _jobManager.AddBackupJob(job);

            var jobs = _jobManager.GetAllJobs();
            Assert.That(jobs.Count, Is.EqualTo(1));
            Assert.That(jobs[0].Name, Is.EqualTo("Job1"));

            // Check that SaveJobs was called with the right list
            _mockConfigManager.Verify(x => x.SaveJobs(It.IsAny<List<BackupJob>>()), Times.Once);
        }

        [Test]
        public void AddBackupJob_ShouldThrow_WhenJobWithSameNameExists()
        {
            var job1 = new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full };
            _jobManager.AddBackupJob(job1);

            var job2 = new BackupJob { Name = "Job1", Id = "2", SourceDirectory = "src2", TargetDirectory = "tgt2", Type = Core.Utils.BackupType.Full };

            Assert.Throws<InvalidOperationException>(() => _jobManager.AddBackupJob(job2));
        }
        
        [Test]
        public void UpdateBackupJob_ShouldUpdateJob_WhenJobExists()
        {
            // Arrange
            var job = new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full };
            _jobManager.AddBackupJob(job);

            // Act
            job.Name = "UpdatedJob";
            _jobManager.UpdateBackupJob(job);

            // Assert
            var jobs = _jobManager.GetAllJobs();
            Assert.That(jobs.Count, Is.EqualTo(1));
            Assert.That(jobs[0].Name, Is.EqualTo("UpdatedJob"));
            _mockConfigManager.Verify(x => x.SaveJobs(It.IsAny<List<BackupJob>>()), Times.Exactly(2)); // Once for Add, once for Update
        }

        [Test]
        public void UpdateBackupJob_ShouldThrow_WhenJobNotFound()
        {
            var job = new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full };
            Assert.Throws<InvalidOperationException>(() => _jobManager.UpdateBackupJob(job));
        }

        [Test]
        public void DeleteBackupJob_ShouldRemoveJob_WhenJobExists()
        {
            var job = new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full };
            _jobManager.AddBackupJob(job);

            var result = _jobManager.DeleteBackupJob(job.Id);

            Assert.That(result, Is.True);
            Assert.That(_jobManager.GetAllJobs().Count, Is.EqualTo(0));
            _mockConfigManager.Verify(x => x.SaveJobs(It.IsAny<List<BackupJob>>()), Times.Exactly(2)); // Add + Delete
        }

        [Test]
        public void DeleteBackupJob_ShouldReturnFalse_WhenJobDoesNotExist()
        {
            var result = _jobManager.DeleteBackupJob("not_exist_id");
            Assert.That(result, Is.False);
            _mockConfigManager.Verify(x => x.SaveJobs(It.IsAny<List<BackupJob>>()), Times.Never);
        }

        [Test]
        public void JobExists_ShouldReturnTrueIfJobWithNameExists()
        {
            var job = new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full };
            _jobManager.AddBackupJob(job);
            Assert.That(_jobManager.JobExists("Job1"), Is.True);
            Assert.That(_jobManager.JobExists("OtherJob"), Is.False);
        }

        [Test]
        public void GetJobCount_ShouldReturnNumberOfJobs()
        {
            Assert.That(_jobManager.GetJobCount(), Is.EqualTo(0));
            _jobManager.AddBackupJob(new BackupJob { Name = "Job1", Id = "1", SourceDirectory = "src", TargetDirectory = "tgt", Type = Core.Utils.BackupType.Full });
            Assert.That(_jobManager.GetJobCount(), Is.EqualTo(1));
        }
        
        [Test]
        public async Task ExecuteBackupJob_CopiesFilesAndReportsProgress()
        {
            // Arrange: Create temp source/target dirs and file
            string tempSource = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string tempTarget = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempSource);
            Directory.CreateDirectory(tempTarget);

            string sourceFile = Path.Combine(tempSource, "testfile.txt");
            string fileContents = "EasySave test!";
            File.WriteAllText(sourceFile, fileContents);

            var job = new BackupJob
            {
                Name = "TestJob",
                Id = Guid.NewGuid().ToString(),
                SourceDirectory = tempSource,
                TargetDirectory = tempTarget,
                Type = BackupType.Full
            };

            var loggerMock = new Mock<ILogger>();
            var configMock = new Mock<IConfigurationManager>();
            var uiServiceMock = new Mock<IUIService>();
            var resourceServiceMock = new Mock<IResourceService>();

            // Config returns no blocking apps, empty encryption, etc.
            configMock.Setup(x => x.LoadJobs()).Returns(new List<BackupJob>());
            configMock.Setup(x => x.GetBlockingApplications()).Returns(new List<string>());
            configMock.Setup(x => x.GetEncryptionFileExtensions()).Returns(new List<string>());
            configMock.Setup(x => x.GetEncryptionWildcard()).Returns(string.Empty);

            var progressValues = new List<float>();
            var progressMock = new Mock<IProgress<float>>();
            progressMock.Setup(p => p.Report(It.IsAny<float>())).Callback<float>(value => progressValues.Add(value));

            var jobManager = new JobManager(loggerMock.Object, configMock.Object, uiServiceMock.Object, resourceServiceMock.Object);
            jobManager.AddBackupJob(job);

            // Act: Run the backup job
            var result = await jobManager.ExecuteBackupJob(job.Id, progressMock.Object, null);

            // Assert: File is copied to target dir
            string targetFile = Path.Combine(tempTarget, "testfile.txt");
            Assert.That(File.Exists(targetFile), Is.True);
            Assert.That(File.ReadAllText(targetFile), Is.EqualTo(fileContents));

            // Assert: Job status is Completed
            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));

            // Assert: Progress was reported
            Assert.That(progressValues, Is.Not.Empty);

            // Assert: Logger and config manager were called
            configMock.Verify(x => x.SaveJobs(It.IsAny<List<BackupJob>>()), Times.AtLeastOnce);
            loggerMock.Verify(x => x.LogBackupOperation(job.Name, sourceFile, targetFile, It.IsAny<long>(), It.IsAny<long>(), "SUCCESS"), Times.Once);

            // Clean up temp files/dirs
            File.Delete(targetFile);
            Directory.Delete(tempTarget);
            File.Delete(sourceFile);
            Directory.Delete(tempSource);
        }
        
        [Test]
        public async Task ExecuteBackupJob_CancelsIfBlockingAppDetected()
        {
            // Arrange temp dirs/files as before, but NO need for real process
            string tempSource = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string tempTarget = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempSource);
            Directory.CreateDirectory(tempTarget);
            File.WriteAllText(Path.Combine(tempSource, "file.txt"), "block me");

            var job = new BackupJob
            {
                Name = "BlockedJob",
                Id = Guid.NewGuid().ToString(),
                SourceDirectory = tempSource,
                TargetDirectory = tempTarget,
                Type = Core.Utils.BackupType.Full
            };

            var loggerMock = new Mock<ILogger>();
            var configMock = new Mock<IConfigurationManager>();
            var uiServiceMock = new Mock<IUIService>();
            var resourceServiceMock = new Mock<IResourceService>();
            var processCheckerMock = new Mock<IProcessChecker>();

            configMock.Setup(x => x.LoadJobs()).Returns(new List<BackupJob>());
            configMock.Setup(x => x.GetBlockingApplications()).Returns(new List<string> { "FakeApp" });
            configMock.Setup(x => x.GetEncryptionFileExtensions()).Returns(new List<string>());
            configMock.Setup(x => x.GetEncryptionWildcard()).Returns(string.Empty);

            // Fake "blocking app is running"
            processCheckerMock.Setup(x => x.IsProcessRunning("FakeApp")).Returns(true);

            var jobManager = new JobManager(
                loggerMock.Object,
                configMock.Object,
                uiServiceMock.Object,
                resourceServiceMock.Object,
                processCheckerMock.Object
            );
            jobManager.AddBackupJob(job);

            // Act
            var result = await jobManager.ExecuteBackupJob(job.Id, null, null);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(job.Status, Is.EqualTo(JobStatus.Canceled));
            loggerMock.Verify(x => x.LogWarning(It.Is<string>(msg => msg.Contains("n'a pas pu demarrer"))), Times.Once);

            // Cleanup
            Directory.Delete(tempTarget, true);
            Directory.Delete(tempSource, true);
        }
    }
}
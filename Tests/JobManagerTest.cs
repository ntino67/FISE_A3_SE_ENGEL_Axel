using NUnit.Framework;
using Moq;
using Core.Model;
using Core.Model.Implementations;
using Core.Model.Interfaces;
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
    }
}

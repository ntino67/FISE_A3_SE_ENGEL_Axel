using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasySave_From_ProSoft.Model;
using EasySave_From_ProSoft.Model.Interfaces;
using EasySave_From_ProSoft.Utils;

namespace EasySave_From_ProSoft.ViewModel
{
    public class JobViewModel
    {
        private readonly IBackupService _jobManager;
        private BackupJob _currentJob;

        public JobViewModel(IBackupService jobManager)
        {
            _jobManager = jobManager;
        }

        public BackupJob CurrentJob
        {
            get { return _currentJob; }
        }

        public void SetCurrentJob(BackupJob job)
        {
            _currentJob = job;
        }

        public void CreateNewJob(string name)
        {
            if (_jobManager.GetJobCount() >= 5)
                throw new InvalidOperationException("Le nombre maximum de jobs (5) est atteint.");

            if (_jobManager.JobExists(name))
                throw new InvalidOperationException($"Un job avec le nom {name} existe déjà.");

            var job = new BackupJob { Name = name };
            _jobManager.AddBackupJob(job);
            _currentJob = job;
        }

        public void UpdateJobName(string newName)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            if (_jobManager.JobExists(newName) && _currentJob.Name != newName)
                throw new InvalidOperationException($"Un job avec le nom {newName} existe déjà.");

            _currentJob.Name = newName;
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public void UpdateSourcePath(string sourcePath)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.SourceDirectory = sourcePath;
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public void UpdateTargetPath(string targetPath)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.TargetDirectory = targetPath;
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public void UpdateBackupType(BackupType type)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.Type = type;
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public async Task<bool> ExecuteCurrentJob()
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            return await _jobManager.ExecuteBackupJob(_currentJob.Id);
        }

        public void ResetCurrentJob()
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.Reset();
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public List<BackupJob> GetAllJobs()
        {
            return _jobManager.GetAllJobs();
        }

        public async Task<List<bool>> ExecuteAllJobs()
        {
            return await _jobManager.ExecuteAllBackupJobs();
        }
    }
}
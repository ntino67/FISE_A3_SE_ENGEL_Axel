using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utils
{
    public class LargeFileTransferManager
    {
        private static readonly Lazy<LargeFileTransferManager> _instance = new Lazy<LargeFileTransferManager>(() => new LargeFileTransferManager());

        public static LargeFileTransferManager Instance => _instance.Value;

        private readonly SemaphoreSlim _largeFileSemaphore = new SemaphoreSlim(3, 3); // 3 transferts simultanés autorisés
        private long _maxFileSizeKB = 1024; // 1 MB par défaut

        private LargeFileTransferManager() { }

        public long MaxFileSizeKB
        {
            get => _maxFileSizeKB;
            set => _maxFileSizeKB = value;
        }

        public bool IsLargeFile(long fileSizeBytes)
        {
            return fileSizeBytes >= _maxFileSizeKB * 1024;
        }

        public async Task<IDisposable> AcquireLargeFileTransferPermissionAsync(CancellationToken cancellationToken = default)
        {
            await _largeFileSemaphore.WaitAsync(cancellationToken);
            return new SemaphoreReleaser(_largeFileSemaphore);
        }

        private class SemaphoreReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _disposed;

            public SemaphoreReleaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_disposed) return;

                _semaphore.Release();
                _disposed = true;
            }
        }
    }
}

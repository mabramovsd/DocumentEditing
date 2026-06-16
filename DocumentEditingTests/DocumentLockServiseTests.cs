using DocumentEditing.Services;
using Xunit;

namespace DocumentEditingTests
{
    public class DocumentLockServiceTests
    {
        private readonly DocumentLockService _lockService = new DocumentLockService();
        private const string FileName = "test.txt";

        [Fact]
        public void TryAcquireWriteLock_ShouldReturnTrue_WhenLockIsFree()
        {
            var result = _lockService.TryAcquireWriteLock(FileName);

            Assert.True(result);
            _lockService.ReleaseWriteLock(FileName); //Clear lock
        }

        [Fact]
        public void TryAcquireWriteLock_ShouldReturnFalse_WhenLockIsAlreadyHeld()
        {
            _lockService.TryAcquireWriteLock(FileName);

            var result = _lockService.TryAcquireWriteLock(FileName);

            Assert.False(result);
            _lockService.ReleaseWriteLock(FileName); //Clear lock
        }

        [Fact]
        public void ReleaseWriteLock_ShouldFreeTheLock()
        {
            _lockService.TryAcquireWriteLock(FileName);
            _lockService.ReleaseWriteLock(FileName);

            var result = _lockService.TryAcquireWriteLock(FileName);

            Assert.True(result);
            _lockService.ReleaseWriteLock(FileName); //Clear lock
        }
    }
}
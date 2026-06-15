namespace DocumentEditing.Services
{
    using System.Collections.Concurrent;
    using System.Threading;

    public class DocumentLockService
    {
        // Потокобезопасный словарь для хранения блокировок по именам файлов.
        private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _locks =
            new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        /// <summary>
        /// Пытается получить блокировку на запись (эксклюзивную).
        /// Возвращает true, если блокировка получена.
        /// </summary>
        public bool TryAcquireWriteLock(string fileName)
        {
            var lockObj = _locks.GetOrAdd(fileName, _ => new ReaderWriterLockSlim());

            try
            {
                // Пробуем немедленно войти в режим записи.
                // Если файл уже кем-то редактируется (кто-то держит блокировку на чтение или запись),
                // этот метод вернет false, а не заблокирует текущий поток.
                return lockObj.TryEnterWriteLock(0);
            }
            catch (Exception ex) when (ex is LockRecursionException || ex is ObjectDisposedException)
            {
                // Обработка редких исключений
                return false;
            }
        }

        /// <summary>
        /// Освобождает блокировку на запись.
        /// </summary>
        public void ReleaseWriteLock(string fileName)
        {
            if (_locks.TryGetValue(fileName, out var lockObj))
            {
                if (lockObj.IsWriteLockHeld)
                {
                    lockObj.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Получает блокировку на чтение (не-эксклюзивную).
        /// Несколько пользователей могут держать эту блокировку одновременно.
        /// </summary>
        public void AcquireReadLock(string fileName)
        {
            var lockObj = _locks.GetOrAdd(fileName, _ => new ReaderWriterLockSlim());
            lockObj.EnterReadLock();
        }

        /// <summary>
        /// Освобождает блокировку на чтение.
        /// </summary>
        public void ReleaseReadLock(string fileName)
        {
            if (_locks.TryGetValue(fileName, out var lockObj))
            {
                if (lockObj.IsReadLockHeld)
                {
                    lockObj.ExitReadLock();
                }
            }
        }
    }
}

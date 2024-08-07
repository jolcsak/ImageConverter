﻿using ImageConverter.Domain;
using ImageConverter.Domain.Dto;
using ImageConverter.Domain.Queue;

namespace ImageConverter.Web.Server.Queue
{
    public class ProcessingQueue : IProcessingQueue
    {
        private const uint MaxSizeQueue = 20;
        private const uint MaxSizePaths = 1;
        private readonly ThreadSafeList<ProcessingQueueItem> processingQueue = new();
        private readonly ThreadSafeList<string> processingPaths = new();

        public ProcessingQueueItem AddQueueItem(string path)
        {
            var newItem = new ProcessingQueueItem(path, ProcessingQueueItemState.Compressing);
            processingQueue.Add(newItem);
            if (processingQueue.Count > MaxSizeQueue)
            {
                processingQueue.RemoveAt(0);
            }
            return newItem;
        }

        public ICollection<ProcessingQueueItem> GetLastQueueItems()
        {
            return processingQueue.ToList();
        }

        public void AddProcessingPath(string? processingPath)
        {
            if (!string.IsNullOrEmpty(processingPath) && !processingPaths.Any(pl => pl.Equals(processingPath)))
            {
                processingPaths.Add(processingPath);
                if (processingPaths.Count > MaxSizePaths)
                {
                    processingPaths.RemoveAt(0);
                }
            }
        }

        public ICollection<string> GetLastProcessingPaths()
        {
            return processingPaths.ToList();
        }

        public void ClearQueue()
        {
            processingPaths.Clear();
            processingQueue.Clear();
        }
    }
}

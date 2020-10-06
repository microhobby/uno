#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Uno;

namespace Windows.Storage.Streams
{
	public partial class RandomAccessStreamReference : IRandomAccessStreamReference
	{
		public static RandomAccessStreamReference CreateFromFile(IStorageFile file)
			=> new RandomAccessStreamReference(file.OpenReadAsync);

		public static RandomAccessStreamReference CreateFromUri(Uri uri)
			=> new RandomAccessStreamReference(async ct
				=>
			{
				var downloader = await StreamedUriDataLoader.Create(ct, uri);
				return new StreamedRandomAccessStream(downloader);
			});

		public static RandomAccessStreamReference CreateFromStream(IRandomAccessStream stream)
			=> new RandomAccessStreamReference(async ct =>
			{
				return new RandomAccessStreamWithContentType(stream.CloneStream());
			});

		private readonly Func<IAsyncOperation<IRandomAccessStreamWithContentType>> _open;

		private RandomAccessStreamReference(Func<IAsyncOperation<IRandomAccessStreamWithContentType>> open)
		{
			_open = open;
		}

		public RandomAccessStreamReference(Func<CancellationToken, Task<IRandomAccessStreamWithContentType>> open)
			: this(() => AsyncOperation.FromTask(open))
		{
		}

		public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync()
			=> _open();
	}
}

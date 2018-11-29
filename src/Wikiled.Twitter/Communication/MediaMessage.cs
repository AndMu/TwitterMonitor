using System;
using System.Collections.Generic;
using Tweetinvi;
using Tweetinvi.Core.Public.Models.Enum;
using Tweetinvi.Core.Public.Parameters;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Wikiled.Twitter.Communication
{
    public class MediaMessage : IFeedMessage
    {
        private IMedia media;

        public MediaMessage(string text, byte[] media)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Media = media ?? throw new ArgumentNullException(nameof(media));
        }

        public string Text { get; }

        public byte[] Media { get; }


        public void Prepare()
        {
            media = Upload.UploadBinary(Media);
            if (media?.MediaId == null || !media.HasBeenUploaded)
            {
                var exception = ExceptionHandler.GetLastException();
                throw new OperationCanceledException("The tweet cannot be published as some of the medias could not be published!");
            }
        }

        public IEnumerable<IPublishTweetParameters> GenerateMessages()
        {
            yield return new PublishTweetParameters(
                Text,
                new PublishTweetOptionalParameters { Medias = new List<IMedia> { media } });
        }
    }
}

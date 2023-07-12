﻿using SyncClipboard.Core.Clipboard;
using SyncClipboard.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using static SyncClipboard.Service.ProfileType;

namespace SyncClipboard.Service
{
    public class UnkonwnProfile : Profile
    {
        public override Core.Clipboard.ProfileType Type => Core.Clipboard.ProfileType.Unknown;

        protected override IClipboardSetter<Profile> ClipboardSetter { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override string ToolTip()
        {
            return "Do not support this type of clipboard";
        }

        public override ClipboardType GetProfileType()
        {
            return ClipboardType.Unknown;
        }

        public override Task UploadProfileAsync(IWebDav webdav, CancellationToken cancelToken)
        {
            return Task.CompletedTask;
        }

        protected override Task<bool> Same(Profile rhs, CancellationToken cancellationToken)
        {
            try
            {
                var _ = (TextProfile)rhs;
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        protected override MetaInfomation CreateMetaInformation()
        {
            throw new System.NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Signer
{
    public enum SIGNING_RESULT
    {
        Success,
        BadInput,
        BadKey,
        SigningFailed,
        NotFoundPrivateKey,
        SigValidateFailed,
        Unknow,
        NotSupport,
        serviceTimeout
    }
    public abstract class BaseSigner
    {
        public abstract bool Verify(byte[] input);
    }
}

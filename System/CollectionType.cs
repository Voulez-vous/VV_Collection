using System;

namespace VV.Collecting
{
#if !VV_COLLECTION_TYPE
    [Serializable]
    public enum CollectionType
    {
        None,
    }
#endif
}
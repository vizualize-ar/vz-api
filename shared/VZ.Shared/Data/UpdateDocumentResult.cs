using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.Data
{
    public enum UpdateDocumentResult
    {
        Success,

        /// <summary>
        /// Update failed because database had newer document. Re-query your document and try again.
        /// </summary>
        Failed_Stale
    }
}

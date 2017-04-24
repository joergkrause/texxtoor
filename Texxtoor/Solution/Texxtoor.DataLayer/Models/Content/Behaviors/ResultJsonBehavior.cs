using System.Collections.Generic;

namespace Texxtoor.DataModels.Attributes {

    /// <summary>
    /// A wrapper that creates the smallest possible JSON for each snippet
    /// </summary>
    public class ResultJsonBehavior
    {
        #region Properties

        public int snippetId { get; set; }
        public string msg { get; set; }
        public int relocateTo { get; set; }
        public IEnumerable<int> snippetsData { get; set; }
        public List<int> children { get; set; }
        public bool success { get; set; }

        #endregion Properties
    }
}

namespace QData.Querable.Extentions
{
    using System;

    [Serializable]
    public class Page
    {
        public object Data { get; set; }

        public int Total { get; set; }
    }
}

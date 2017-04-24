namespace Texxtoor.EasyAuthor.Utilities
{
    using System.Diagnostics.CodeAnalysis;

    public class Constants
    {
        public static class Images
        {
            /// <summary>
            /// String.Format params:
            /// <para>{0} - id</para>
            /// <para>{1} - image type</para>
            /// </summary>
            public const string ApiUrlTemplate = "/api/images/{0}/{1}";

            // ReSharper disable InconsistentNaming
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed. Suppression is OK here.")]
            public static class StandardResolutions
            {
                public const string _150x200 = "150x200";

                public const string _100x150 = "100x150";
            }
            // ReSharper restore InconsistentNaming
            public static class StandardTypes
            {
                public const string ProfileByName = "profilebyname";
                public const string Profile = "profile";
                public const string Home = "home";
                public const string Epub = "epub";
                public const string Reader = "reader";
                public const string Project = "project";
                public const string ProjectCover = "projectcover";
                public const string Team = "team";
                public const string MemberThumbnail = "memberthumbnail";
                public const string FinderResource = "finderresource";
                public const string EditorResource = "editorresource";
                public const string Editor = "editor";
                public const string Barcode = "barcode";
            }
        }
    }
}
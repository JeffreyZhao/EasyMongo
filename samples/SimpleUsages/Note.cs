using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUsages
{
    public class Note
    {
        public string NoteID { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int Version { get; set; }

        public long UserID { get; set; }

        public string CategoryID { get; set; }

        public List<string> Tags { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public NoteContentTypes ContentTypes { get; set; }

        public int CommentCount { get; set; }

        public int SaveAsCount { get; set; }

        public string Abstract { get; set; }

        public bool HasAttachments { get; set; }

        public string SourceUrl { get; set; }

        public bool Encrypted { get; set; }

        public string PasswordReminder { get; set; }

        public string Password { get; set; }

        private bool m_isCryptoText;
        private bool m_isCryptoTextFirstVisited = false;
        public bool IsCryptoText
        {
            get
            {
                if (!this.m_isCryptoTextFirstVisited)
                {
                    this.m_isCryptoText = this.Encrypted;
                    this.m_isCryptoTextFirstVisited = true;
                }

                return this.m_isCryptoText;
            }
            private set
            {
                this.m_isCryptoText = value;
            }
        }

        private string m_searchText;
        public string SearchText
        {
            get
            {
                if (this.Encrypted)
                {
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(this.m_searchText))
                {
                    this.m_searchText = this.Title + " " + this.Content + " " + String.Join(" ", this.Tags.ToArray());
                }

                return this.m_searchText;
            }
        }

        public NoteResourceInlineStatus ResourceInlineStatus { get; set; }
    }

    [Flags]
    public enum NoteContentTypes
    {
        None = 0,
        Text = 1,
        Image = 2,
        Video = 4,
        Audio = 8,
        Flash = 16,
        Url = 32,
        File = 64
    }

    public enum NoteResourceInlineStatus
    {
        Original = 0,
        Processing = 1,
        LockedForUpdate = 2,
        Processed = 3
    }
}

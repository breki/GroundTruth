using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace GroundTruth.Engine.PolishMaps
{
    [SuppressMessage ("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
    public abstract class CGpsMapperWriterBase<T> : IDisposable 
        where T : CGpsMapperWriterBase<T>
    {
        public T Add (string text)
        {
            AppendLine (text);
            return (T)this;
        }

        public T Add (string parameter, string value)
        {
            AppendLine ("{0}={1}", parameter, value);
            return (T)this;
        }

        public T Add (string parameter, int value)
        {
            AppendLine ("{0}={1}", parameter, value);
            return (T)this;
        }

        public T Add (string parameter, float value)
        {
            AppendLine ("{0}={1}", parameter, value);
            return (T)this;
        }

        public T AddFormat (string format, params object[] args)
        {
            AppendLine (format, args);
            return (T)this;
        }

        public T AddHexValue (string parameter, int value)
        {
            AppendLine ("{0}=0x{1:x}", parameter, value);
            return (T)this;
        }

        public T AddSection (string sectionId)
        {
            CloseSection ();

            AppendLine ("[{0}]", sectionId);
            AppendLine ();
            CurrentSectionId = sectionId;

            return (T)this;
        }

        public void FinishMap ()
        {
            CloseSection ();
        }

        protected CGpsMapperWriterBase (Stream outputStream, string mapFileInfo)
        {
            this.mapFileInfo = mapFileInfo;
            this.streamWriter = new StreamWriter (outputStream, new UTF8Encoding (false));
            WriteFileHeader();
        }

        protected string CurrentSectionId
        {
            get { return currentSectionId; }
            set { currentSectionId = value; }
        }

        protected void AppendLine ()
        {
            streamWriter.WriteLine ();
        }

        protected void AppendLine (string format, params object[] args)
        {
            streamWriter.WriteLine (format, args);
        }

        protected abstract void CloseSection();

        protected void WriteFileHeader()
        {
            System.Diagnostics.FileVersionInfo version = System.Diagnostics.FileVersionInfo.GetVersionInfo
                (System.Reflection.Assembly.GetExecutingAssembly ().Location);

            Add (";");
            Add ("; Generated using GroundTruth software (http://wiki.openstreetmap.org/wiki/GroundTruth)");
            AddFormat("; GroundTruth was created by Igor Brejc", version.FileVersion);
            AddFormat("; GroundTruth version: {0}", version.FileVersion);
            AddFormat("; Time of generating: {0}", DateTime.Now);
            AddFormat("; Map file info: {0}", mapFileInfo);
            Add (";");
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">If <code>false</code>, cleans up native resources. 
        /// If <code>true</code> cleans up both managed and native resources</param>
        protected virtual void Dispose (bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                    if (streamWriter != null)
                        streamWriter.Dispose ();
                }

                disposed = true;
            }
        }

        private bool disposed;

        #endregion

        private string currentSectionId;
        private string mapFileInfo;
        private StreamWriter streamWriter;        
    }
}
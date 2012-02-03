using System.Diagnostics.CodeAnalysis;
using System.IO;
using GroundTruth.Engine.PolishMaps;

namespace GroundTruth.Engine
{
    [SuppressMessage ("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
    public class CGpsMapperGeneralFileWriter : CGpsMapperWriterBase<CGpsMapperGeneralFileWriter>
    {
        public CGpsMapperGeneralFileWriter (
            Stream outputStream, 
            bool useTypStyleSectionClosing,
            string mapFileInfo)
            : base (outputStream, mapFileInfo)
        {
            this.useTypStyleSectionClosing = useTypStyleSectionClosing;
        }

        protected override void CloseSection ()
        {
            if (CurrentSectionId != null)
            {
                if (useTypStyleSectionClosing)
                    AppendLine ("[end]", CurrentSectionId);
                else
                    AppendLine ("[END-{0}]", CurrentSectionId);

                AppendLine ();
            }
        }

        private readonly bool useTypStyleSectionClosing;
    }
}
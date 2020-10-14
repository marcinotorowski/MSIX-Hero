using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PriFormat
{
    public class ReferencedFileSection : Section
    {
        public IReadOnlyList<ReferencedFile> ReferencedFiles { get; private set; }

        internal const string Identifier = "[def_file_list]\0";

        internal ReferencedFileSection(PriFile priFile) : base(Identifier, priFile)
        {
        }

        protected override bool ParseSectionContent(BinaryReader binaryReader)
        {
            ushort numRoots = binaryReader.ReadUInt16();
            ushort numFolders = binaryReader.ReadUInt16();
            ushort numFiles = binaryReader.ReadUInt16();
            binaryReader.ExpectUInt16(0);
            uint totalDataLength = binaryReader.ReadUInt32();

            List<FolderInfo> folderInfos = new List<FolderInfo>(numFolders);

            for (int i = 0; i < numFolders; i++)
            {
                binaryReader.ExpectUInt16(0);
                ushort parentFolder = binaryReader.ReadUInt16();
                ushort numFoldersInFolder = binaryReader.ReadUInt16();
                ushort firstFolderInFolder = binaryReader.ReadUInt16();
                ushort numFilesInFolder = binaryReader.ReadUInt16();
                ushort firstFileInFolder = binaryReader.ReadUInt16();
                ushort folderNameLength = binaryReader.ReadUInt16();
                ushort fullPathLength = binaryReader.ReadUInt16();
                uint folderNameOffset = binaryReader.ReadUInt32();
                folderInfos.Add(new FolderInfo(parentFolder, numFoldersInFolder, firstFolderInFolder, numFilesInFolder, firstFileInFolder, folderNameLength, fullPathLength, folderNameOffset));
            }

            List<FileInfo> fileInfos = new List<FileInfo>(numFiles);

            for (int i = 0; i < numFiles; i++)
            {
                binaryReader.ReadUInt16();
                ushort parentFolder = binaryReader.ReadUInt16();
                ushort fullPathLength = binaryReader.ReadUInt16();
                ushort fileNameLength = binaryReader.ReadUInt16();
                uint fileNameOffset = binaryReader.ReadUInt32();
                fileInfos.Add(new FileInfo(parentFolder, fullPathLength, fileNameLength, fileNameOffset));
            }

            long dataStartPosition = binaryReader.BaseStream.Position;

            List<ReferencedFolder> referencedFolders = new List<ReferencedFolder>(numFolders);

            for (int i = 0; i < numFolders; i++)
            {
                binaryReader.BaseStream.Seek(dataStartPosition + folderInfos[i].FolderNameOffset * 2, SeekOrigin.Begin);

                string name = binaryReader.ReadString(Encoding.Unicode, folderInfos[i].FolderNameLength);

                referencedFolders.Add(new ReferencedFolder(null, name));
            }

            for (int i = 0; i < numFolders; i++)
                if (folderInfos[i].ParentFolder != 0xFFFF)
                    referencedFolders[i].Parent = referencedFolders[folderInfos[i].ParentFolder];

            List<ReferencedFile> referencedFiles = new List<ReferencedFile>(numFiles);

            for (int i = 0; i < numFiles; i++)
            {
                binaryReader.BaseStream.Seek(dataStartPosition + fileInfos[i].FileNameOffset * 2, SeekOrigin.Begin);

                string name = binaryReader.ReadString(Encoding.Unicode, fileInfos[i].FileNameLength);

                ReferencedFolder parentFolder;

                if (fileInfos[i].ParentFolder != 0xFFFF)
                    parentFolder = referencedFolders[fileInfos[i].ParentFolder];
                else
                    parentFolder = null;

                referencedFiles.Add(new ReferencedFile(parentFolder, name));
            }

            for (int i = 0; i < numFolders; i++)
            {
                List<ReferencedEntry> children = new List<ReferencedEntry>(folderInfos[i].NumFoldersInFolder + folderInfos[i].NumFilesInFolder);

                for (int j = 0; j < folderInfos[i].NumFoldersInFolder; j++)
                    children.Add(referencedFolders[folderInfos[i].FirstFolderInFolder + j]);

                for (int j = 0; j < folderInfos[i].NumFilesInFolder; j++)
                    children.Add(referencedFiles[folderInfos[i].FirstFileInFolder + j]);

                referencedFolders[i].Children = children;
            }

            ReferencedFiles = referencedFiles;

            return true;
        }

        private struct FolderInfo
        {
            public ushort ParentFolder;
            public ushort NumFoldersInFolder;
            public ushort FirstFolderInFolder;
            public ushort NumFilesInFolder;
            public ushort FirstFileInFolder;
            public ushort FolderNameLength;
            public ushort FullPathLength;
            public uint FolderNameOffset;

            public FolderInfo(ushort parentFolder, ushort numFoldersInFolder, ushort firstFolderInFolder, ushort numFilesInFolder, ushort firstFileInFolder, ushort folderNameLength, ushort fullPathLength, uint folderNameOffset)
            {
                ParentFolder = parentFolder;
                NumFoldersInFolder = numFoldersInFolder;
                FirstFolderInFolder = firstFolderInFolder;
                NumFilesInFolder = numFilesInFolder;
                FirstFileInFolder = firstFileInFolder;
                FolderNameLength = folderNameLength;
                FullPathLength = fullPathLength;
                FolderNameOffset = folderNameOffset;
            }
        }

        private struct FileInfo
        {
            public ushort ParentFolder;
            public ushort FullPathLength;
            public ushort FileNameLength;
            public uint FileNameOffset;

            public FileInfo(ushort parentFolder, ushort fullPathLength, ushort fileNameLength, uint fileNameOffset)
            {
                ParentFolder = parentFolder;
                FullPathLength = fullPathLength;
                FileNameLength = fileNameLength;
                FileNameOffset = fileNameOffset;
            }
        }
    }

    public class ReferencedEntry
    {
        public ReferencedFolder Parent { get; internal set; }
        public string Name { get; }

        internal ReferencedEntry(ReferencedFolder parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        string fullName;

        public string FullName
        {
            get
            {
                if (fullName == null)
                    if (Parent == null)
                        fullName = Name;
                    else
                        fullName = Parent.FullName + "\\" + Name;

                return fullName;
            }
        }
    }

    public class ReferencedFolder : ReferencedEntry
    {
        internal ReferencedFolder(ReferencedFolder parent, string name) : base(parent, name)
        {
        }

        public IReadOnlyList<ReferencedEntry> Children { get; internal set; }
    }

    public class ReferencedFile : ReferencedEntry
    {
        internal ReferencedFile(ReferencedFolder parent, string name) : base(parent, name)
        {
        }
    }

    public struct ReferencedFileRef
    {
        internal int fileIndex;

        internal ReferencedFileRef(int fileIndex)
        {
            this.fileIndex = fileIndex;
        }
    }
}

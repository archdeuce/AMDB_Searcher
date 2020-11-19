namespace AMDB_Searcher.Model
{
    public class Device
    {
        public string InventoryId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        public Device(string inventoryId, string name, string comment)
        {
            this.InventoryId = inventoryId;
            this.Name = name;
            this.Comment = comment;
        }

        public override string ToString()
        {
            //string showTitle = $"{this.InventoryId} - {this.Name}";
            string showTitle = string.Format("{0, -8} {1, -3} {2, -20}", this.InventoryId, "-", this.Name);
            return showTitle;
        }
    }
}

/*
0 - Id
1 - Object_Type
2 - InventoryNumber
3 - EquipmentName
4 - SerialNumber
5 - Status
6 - User_Id
7 - Office
8 - Room
9 - Hostname
10 - ParentServer
11 - IP
12 - MAC
13 - CPU
14 - DepartmentUser
15 - Video
16 - MotherBoard
17 - License
18 - CurrentOS
19 - AcquisitionDate
20 - LicenseType
21 - Comment
22 - Manager
23 - LastDateUpdated
24 - HDD0
25 - HDD1
26 - HDD2
27 - HDD3
28 - RAM
29 - ScreenSize
30 - IsDeleted
*/
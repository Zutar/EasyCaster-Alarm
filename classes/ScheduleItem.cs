using System;

namespace EasyCaster_Alarm.classes
{
    [Serializable()]
    class ScheduleItem
    {
        public int select_start { get; set; }
        public int select_end { get; set; }
        public string app_list_item { get; set; }
        public int key_press { get; set; }
        public string ext_app { get; set; }
        public int frequency { get; set; } 

        public ScheduleItem(int select_start, int select_end, string app_list_item, int key_press, string ext_app, int frequency)
        {
            this.select_start = select_start;
            this.select_end = select_end;
            this.app_list_item = app_list_item;
            this.key_press = key_press;
            this.ext_app = ext_app;
            this.frequency = frequency;
        } 
    }
}

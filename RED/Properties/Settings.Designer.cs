﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RED.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool InputAutoDeadzone {
            get {
                return ((bool)(this["InputAutoDeadzone"]));
            }
            set {
                this["InputAutoDeadzone"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int InputManualDeadzone {
            get {
                return ((int)(this["InputManualDeadzone"]));
            }
            set {
                this["InputManualDeadzone"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double GPSBaseStationLocationLatitude {
            get {
                return ((double)(this["GPSBaseStationLocationLatitude"]));
            }
            set {
                this["GPSBaseStationLocationLatitude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double GPSBaseStationLocationLongitude {
            get {
                return ((double)(this["GPSBaseStationLocationLongitude"]));
            }
            set {
                this["GPSBaseStationLocationLongitude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("38.406426")]
        public double GPSStartLocationLatitude {
            get {
                return ((double)(this["GPSStartLocationLatitude"]));
            }
            set {
                this["GPSStartLocationLatitude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-110.791919")]
        public double GPSStartLocationLongitude {
            get {
                return ((double)(this["GPSStartLocationLongitude"]));
            }
            set {
                this["GPSStartLocationLongitude"] = value;
            }
        }
    }
}

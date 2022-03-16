﻿#pragma checksum "..\..\..\..\AuthWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "4BF19CC4C8966454A9ED4C78B7DD5DD9A4D5E047"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using EasyCaster_Alarm;
using FontAwesome.WPF;
using FontAwesome.WPF.Converters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace EasyCaster_Alarm {
    
    
    /// <summary>
    /// AuthWindow
    /// </summary>
    public partial class AuthWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 1 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal EasyCaster_Alarm.AuthWindow auth_win;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label auth_title;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel auth_verification_block;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox auth_verification;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox auth_password;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox auth_phone;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal FontAwesome.WPF.ImageAwesome auth_spinner;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button auth_submit;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button auth_reset;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label error_msg;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\..\AuthWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label success_msg;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.12.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/EasyCaster Alarm;V1.0.3.9;component/authwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\AuthWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.12.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.auth_win = ((EasyCaster_Alarm.AuthWindow)(target));
            
            #line 9 "..\..\..\..\AuthWindow.xaml"
            this.auth_win.Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\..\AuthWindow.xaml"
            this.auth_win.KeyUp += new System.Windows.Input.KeyEventHandler(this.auth_win_KeyUp);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\..\AuthWindow.xaml"
            this.auth_win.Loaded += new System.Windows.RoutedEventHandler(this.auth_win_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.auth_title = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.auth_verification_block = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.auth_verification = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.auth_password = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.auth_phone = ((System.Windows.Controls.TextBox)(target));
            
            #line 64 "..\..\..\..\AuthWindow.xaml"
            this.auth_phone.LostFocus += new System.Windows.RoutedEventHandler(this.auth_phone_LostFocus);
            
            #line default
            #line hidden
            return;
            case 7:
            this.auth_spinner = ((FontAwesome.WPF.ImageAwesome)(target));
            return;
            case 8:
            this.auth_submit = ((System.Windows.Controls.Button)(target));
            
            #line 66 "..\..\..\..\AuthWindow.xaml"
            this.auth_submit.Click += new System.Windows.RoutedEventHandler(this.auth_submit_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.auth_reset = ((System.Windows.Controls.Button)(target));
            
            #line 69 "..\..\..\..\AuthWindow.xaml"
            this.auth_reset.Click += new System.Windows.RoutedEventHandler(this.auth_reset_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.error_msg = ((System.Windows.Controls.Label)(target));
            return;
            case 11:
            this.success_msg = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


﻿using Microsoft.AspNetCore.Components.Web.Virtualization;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class ReportComponentModel
    {
        public enum Control
        {
            None,
            Label,
            Table,
            Band,
            Layer,
            Report
        }
        public Control Type { get; set; } = Control.None;

        public bool Locked { get; set; } = false;

        public ReportComponentModel() {
            this.Uid = Guid.NewGuid().ToString();
        }
        public int TabIndex { get; set; } = 0;

        public string ParentUid { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;

        public bool IsGroup { get { return Name != null; } }

        public int ZIndex { get; set; }
        /// <summary>
        /// 일반 X, Y 값은 Div에 그려질 상대값이 설정되며
        /// Absolute는 Report 기준으로 생성되는 절대값을 표현한다. 
        /// 업데이트 시점 : 
        /// 1. 처음 추가될때
        /// 2. 리사이즈 될때
        /// 3. 마우스 업 이벤트 발생할때
        /// </summary>
        public int AbsoluteOffsetX { get; set; } = 0;
        public int AbsoluteOffsetY { get; set; } = 0;
        public int AbsoluteOffsetRight { get; set; } = 0;
        public int AbsoluteOffsetBottom { get; set; } = 0;

        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;
        public virtual int Right { get { return X + Width; } set { } }
        public virtual int Bottom { get { return Y + Height; } set { } }
        private bool selected = false;
        public bool Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                if (value == true)
                {
                }
                else
                {
                }
            }
        }

        public bool IsEditMode { get; set; } = false;

        public bool Visible
        {
            get { return !Hidden; }
            set { Hidden = !value; }
        }
        public bool Hidden { get; set; } = false;
        public Margin Margin { get; set; } = new Margin();

        public bool IsOverlap { get; set; } = false;


        public string BackColor { get; set; } = "rgba(0,0,0,0)";
        public string ForeColor { get; set; } = "Black";

        public Border Border { get; set; } = new Border();

        public string Text { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;

        public Font Font { get; set; } = new Font();

        public List<ReportComponentModel> Children { get; set; } = new List<ReportComponentModel>();

        /// <summary>
        /// C#에서는 깊은복사(참조복사)가 없다. 왜 아직도?? 왜?? 느려도 만들어줘야지..
        /// 그래서 얕은복사(값만복사) 이 후에 참조된값들을 각각 복사한다.
        /// </summary>
        /// <returns></returns>
        public ReportComponentModel DeepClone()
        {
            var thisModel = (ReportComponentModel) this.MemberwiseClone();
            thisModel.Margin = (Margin)Margin.Clone();
            thisModel.Border = (Border)Border.Clone();
            thisModel.Font = (Font)Font.Clone();
            return thisModel;

            //todo : DeepClone - Children 이 있다면 그것도 복사해줘야 함.
        }

    }
}

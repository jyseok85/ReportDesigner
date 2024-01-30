using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.EtcComponents
{
    /// <summary>
    /// 폰트모델에 넣기 애매한 , 텍스트 속성들이 포함된다.
    /// </summary>
    public class Paragraph : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// 수직 정렬
        /// </summary>
        public string VerticalAlignment { get; set; } = "center";

        /// <summary>
        /// 수평 정렬
        /// </summary>
        public string HorizontalAlignment { get; set; } = "center";

        /// <summary>
        /// [고급] 줄간격(LineSpacing)
        /// </summary>
        public float LineHeight { get; set; } = 1;

        /// <summary>
        /// [고급] 텍스트 방향
        /// </summary>
        public TextDirection TextDirection { get; set; } = TextDirection.Horizontal;

        /// <summary>
        /// [고급] 텍스트가 컨트롤 사이즈를 넘어갔을 경우 자동 개행 여부
        /// </summary>
        public bool AutoLineBreak { get; set; } = false;

        /// <summary>
        /// [고급] 글자간격
        /// </summary>
        public float CharacterSpacing { get; set; }

    }
}

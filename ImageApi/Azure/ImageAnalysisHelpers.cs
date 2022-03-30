using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ImageApi.Azure;
public class ImageAnalysisHelpers
{
    public class AnalysisQueryData
    {
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
        public string ImageHash { get; set; }
        public string ClientIP { get; set; }
    }
    
    public static string ImageAnalysisToSql(ImageAnalysis analysis, AnalysisQueryData queryData)
    {
        void CreateAnalysis(StringBuilder sb)
        {
            var date = DateTime.Now.ToString("s").Replace("T", " ");
            sb.AppendLine($"insert into `image` values (0, '{queryData.ImageUrl})', '{queryData.ImageName}', '{queryData.ImageHash}');");
            sb.AppendLine($"insert into `analyse` values (0, LAST_INSERT_ID(), '{queryData.ClientIP}', '{date}', '{date}');");
            sb.AppendLine("set @analyse_id = LAST_INSERT_ID();");
        }

        void CreateObject(StringBuilder sb, string objectCategory, string objectName)
        {
            sb.AppendLine($"insert into `object` values (0, @analyse_id, '{objectCategory}', '{objectName}');");
        }

        void OpenAttribute(StringBuilder sb)
        {
            sb.AppendLine($"insert into `attribute` values ");
        }

        void CloseAttribute(StringBuilder sb)
        {
            sb.Remove(sb.Length - 3, 1);
            sb.Append(';');
        }

        void AddStringAttributeValue(StringBuilder sb, string key, string value, string parentObjectId = "LAST_INSERT_ID()")
        {
            sb.AppendLine($"(0, {parentObjectId}, '{key}', 'string', '{value}', null, null),");
        }

        void AddBoolAttributeValue(StringBuilder sb, string key, bool value, string parentObjectId = "LAST_INSERT_ID()")
        {
            sb.AppendLine($"(0, {parentObjectId}, '{key}', 'boolean', null, null, {(value ? "1" : "0")}),");
        }

        void AddNumberAttributeValue(StringBuilder sb, string key, double value, string parentObjectId = "LAST_INSERT_ID()")
        {
            sb.AppendLine($"(0, {parentObjectId}, '{key}', 'number', null, {value}, null),");
        }

        void AddFaceRectAttributeValues(StringBuilder sb, string key, FaceRectangle value, string parentObjectId = "LAST_INSERT_ID()")
        {
            AddNumberAttributeValue(sb, $"{key}_left", value.Left);
            AddNumberAttributeValue(sb, $"{key}_top", value.Top);
            AddNumberAttributeValue(sb, $"{key}_width", value.Width);
            AddNumberAttributeValue(sb, $"{key}_height", value.Height);
        }

        void AddBoundingRectAttributeValues(StringBuilder sb, string key, BoundingRect value, string parentObjectId = "LAST_INSERT_ID()")
        {
            AddNumberAttributeValue(sb, $"{key}_left", value.X);
            AddNumberAttributeValue(sb, $"{key}_top", value.Y);
            AddNumberAttributeValue(sb, $"{key}_width", value.W);
            AddNumberAttributeValue(sb, $"{key}_height", value.H);
        }

        var sql = new StringBuilder();
        CreateAnalysis(sql);

        // Categories
        foreach(var category in analysis.Categories)
        {
            CreateObject(sql, "categories", "");
            OpenAttribute(sql);
            AddNumberAttributeValue(sql, category.Name, category.Score);
            CloseAttribute(sql);
        }

        // Adult
        CreateObject(sql, "adult", "");
        OpenAttribute(sql);

        AddBoolAttributeValue(sql, "isAdultContent", analysis.Adult.IsAdultContent);
        AddBoolAttributeValue(sql, "isRacyContent", analysis.Adult.IsRacyContent);
        AddBoolAttributeValue(sql, "isGoryContent", analysis.Adult.IsGoryContent);
        AddNumberAttributeValue(sql, "adultScore", analysis.Adult.AdultScore);
        AddNumberAttributeValue(sql, "racyScore", analysis.Adult.RacyScore);
        AddNumberAttributeValue(sql, "goryScore", analysis.Adult.GoreScore);

        CloseAttribute(sql);

        // Color 
        CreateObject(sql, "color", "");
        OpenAttribute(sql);

        AddStringAttributeValue(sql, "dominantColorForeground", analysis.Color.DominantColorForeground);
        AddStringAttributeValue(sql, "dominantColorBackground", analysis.Color.DominantColorBackground);
        AddStringAttributeValue(sql, "accentColor", analysis.Color.AccentColor);
        AddBoolAttributeValue(sql, "isBWImg", analysis.Color.IsBWImg);
        foreach(var dominantColor in analysis.Color.DominantColors)
        {
            AddStringAttributeValue(sql, "dominantColors", dominantColor);
        }

        CloseAttribute(sql);

        // Image type
        CreateObject(sql, "imageType", "");
        OpenAttribute(sql);
        AddNumberAttributeValue(sql, "clipArtType", analysis.ImageType.ClipArtType);
        AddNumberAttributeValue(sql, "lineDrawingType", analysis.ImageType.LineDrawingType);
        CloseAttribute(sql);

        // Tags
        foreach(var tag in analysis.Tags)
        {
            CreateObject(sql, "imageType", "");
            OpenAttribute(sql);
            AddNumberAttributeValue(sql, tag.Name, tag.Confidence);
            CloseAttribute(sql);
        }

        // Description
        CreateObject(sql, "description", "tags");
        OpenAttribute(sql);
        foreach(var tag in analysis.Description.Tags)
        {
            AddNumberAttributeValue(sql, tag, 1);
        }

        CloseAttribute(sql);

        CreateObject(sql, "description", "captions");
        OpenAttribute(sql);
        foreach(var caption in analysis.Description.Captions)
        {
            AddStringAttributeValue(sql, "text", caption.Text);
            AddNumberAttributeValue(sql, "confidence", caption.Confidence);
        }

        CloseAttribute(sql);

        // Faces 
        foreach(var face in analysis.Faces)
        {
            CreateObject(sql, "faces", "");
            OpenAttribute(sql);
            AddNumberAttributeValue(sql, "age", face.Age);
            AddStringAttributeValue(sql, "gender", face.Gender.ToString() ?? "");
            AddFaceRectAttributeValues(sql, "faceRectangle", face.FaceRectangle);
            CloseAttribute(sql);
        }

        // Faces 
        foreach(var obj in analysis.Objects)
        {
            CreateObject(sql, "objects", "");
            OpenAttribute(sql);
            AddStringAttributeValue(sql, "objectProperty", obj.ObjectProperty);
            AddNumberAttributeValue(sql, "confidence", obj.Confidence);
            AddBoundingRectAttributeValues(sql, "rectangle", obj.Rectangle);
            CloseAttribute(sql);
        }


        return sql.ToString();
    }
}

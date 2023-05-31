using HandlebarsDotNet;
using System.Globalization;
using System.Text;

namespace Acies.Docs.Services.HandlebarHelpers;

public static class HandlebarInjectionHelper
{
    private static readonly string Directory = OperatingSystem.IsWindows() ? "tmp" : "/tmp";

    public static void RegisterHelpers()
    {
        RegisterDefaultHelpers();
    }

    private static void RegisterDefaultHelpers()
    {
        // {{math-multiply this.Quantity this.UnitPrice}}
        Handlebars.RegisterHelper("math-multiply", (writer, _, parameters) =>
        {
            var valueOne = parameters.Skip(0).FirstOrDefault();
            if (!double.TryParse($"{valueOne}", out var fValueOne))
            {
                writer.WriteSafeString("-1");
                return;
            }

            var valueTwo = parameters.Skip(1).FirstOrDefault();
            if (!double.TryParse($"{valueTwo}", out var fValueTwo))
            {
                writer.WriteSafeString("-1");
                return;
            }

            writer.WriteSafeString($"{fValueOne * fValueTwo}");
        });

        // {{math-mod @index 2}}
        Handlebars.RegisterHelper("math-mod", (writer, _, parameters) =>
        {
            var valueOne = parameters.Skip(0).FirstOrDefault();
            if (!double.TryParse($"{valueOne}", out var fValueOne))
            {
                writer.WriteSafeString("-1");
                return;
            }

            var valueTwo = parameters.Skip(1).FirstOrDefault();
            if (!double.TryParse($"{valueTwo}", out var fValueTwo))
            {
                writer.WriteSafeString("-1");
                return;
            }

            writer.WriteSafeString($"{fValueOne % fValueTwo}");
        });

        // {{format-number this.Quantity 2 'da-DK'}}
        Handlebars.RegisterHelper("format-number", (writer, _, parameters) =>
        {
            var value = parameters.Skip(0).FirstOrDefault();
            if (!double.TryParse($"{value}", out var fValue))
            {
                writer.WriteSafeString("-1");
                return;
            }

            var decimals = parameters.Skip(1).FirstOrDefault();
            if (!int.TryParse($"{decimals}", out var fDecimals))
            {
                writer.WriteSafeString("-1");
                return;
            }

            var cultureName = $"{parameters.Skip(2).FirstOrDefault()}";

            if (cultureName.Equals("") || !CultureInfo.GetCultures(CultureTypes.AllCultures).Any(x => string.Equals(x.Name, cultureName, StringComparison.CurrentCultureIgnoreCase)))
            {
                writer.WriteSafeString("-1");
                return;
            }

            var formatted = fValue.ToString($"N{fDecimals}", new CultureInfo(cultureName));

            writer.WriteSafeString(formatted);
        });

        // {{format-date this.Head.CorrectionDeadline}}
        Handlebars.RegisterHelper("format-date", (writer, _, parameters) =>
        {
            var value = $"{parameters.Skip(0).FirstOrDefault()}";

            var format = $"{parameters.Skip(1).FirstOrDefault()}";

            if (!DateTime.TryParse(value, out var date))
            {
                writer.WriteSafeString("-1");
                return;
            }

            var formatted = date.ToString(format, CultureInfo.InvariantCulture);

            writer.WriteSafeString(formatted);
        });

        // {{inject-local-image this.DrawingUrl}}
        Handlebars.RegisterHelper("inject-local-image", (writer, _, parameters) =>
        {
            var path = $"{Directory}/{parameters.FirstOrDefault()}";
            var fileName = $"{Path.GetFileNameWithoutExtension(path)}".ToLower();
            var fileExtension = $"{Path.GetExtension(path).Replace(".", "")}".ToLower();

            if (path.Equals("") || fileName.Equals("") || fileExtension.Equals(""))
            {
                return;
            }

            if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
            {
                return;
            }

            if (!File.Exists(path))
            {
                writer.WriteSafeString(path);
                return;
            }

            var file = File.ReadAllBytes(path);
            var base64 = Convert.ToBase64String(file);
            var image = $"data:image/{(fileExtension == "svg" ? "svg+xml" : fileExtension)};charset=utf-8;base64,{base64}";

            writer.WriteSafeString(image);
        });

        // {{inject-local-svg this.DrawingUrl}}
        Handlebars.RegisterHelper("inject-local-svg", (writer, _, parameters) =>
        {
            var path = $"{Directory}/{parameters.FirstOrDefault()}";
            var fileName = $"{Path.GetFileNameWithoutExtension(path)}";
            var fileExtension = $"{Path.GetExtension(path).Replace(".", "")}";

            if (path.Equals("") || fileName.Equals("") || fileExtension.Equals(""))
            {
                return;
            }
            
            if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
            {
                return;
            }

            if (!File.Exists(path))
            {
                writer.WriteSafeString(path);
                return;
            }

            var file = File.ReadAllText(path, Encoding.UTF8);

            if (fileExtension.Equals("svg"))
            {
                writer.WriteSafeString(file);
            }
        });

        // {{specific-orderline-sumproducts this.OrderLines}}
        Handlebars.RegisterHelper("specific-orderline-sumproducts", (writer, _, parameters) =>
        {
            var list = parameters.FirstOrDefault() as List<dynamic>;

            var sum = 0;

            list?.ForEach(e => sum += e.quantity * e.unitPrice);

            writer.WriteSafeString($"{sum}");
        });
    }
}
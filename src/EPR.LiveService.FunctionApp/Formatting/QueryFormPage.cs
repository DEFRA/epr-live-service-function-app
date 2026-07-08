using System.Net;
using System.Text;
using System.Text.Json;
using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class QueryFormPage
{
    public static string Build(QueryDefinition definition)
    {
        var fields = new StringBuilder();
        foreach (var param in definition.Parameters)
        {
            var inputType = param.Type switch
            {
                "number" => "number",
                "date" => "date",
                _ => "text"
            };

            fields.AppendLine($"""
                <label for="{param.Name}">{WebUtility.HtmlEncode(param.Label)}</label>
                <input type="{inputType}" id="{param.Name}" name="{param.Name}" {(param.Required ? "required" : "")} />
                """);
        }

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <title>{{WebUtility.HtmlEncode(definition.DisplayName)}}</title>
            <style>
                body { font-family: system-ui, sans-serif; max-width: 700px; margin: 2rem auto; background: #1e1e1e; color: #d4d4d4; }
                label { display: block; margin-top: 0.75rem; font-weight: 600; }
                input { width: 100%; padding: 0.4rem; margin-top: 0.25rem; box-sizing: border-box; }
                button { margin-top: 1.25rem; padding: 0.5rem 1.2rem; }
                #results { margin-top: 1.5rem; }
                pre { overflow-x: auto; }
                .error { color: #ff6b6b; }
            </style>
        </head>
        <body>
            <h1>{{WebUtility.HtmlEncode(definition.DisplayName)}}</h1>
            <p>{{WebUtility.HtmlEncode(definition.Description)}}</p>

            <form id="query-form">
                {{fields}}
                <button type="submit">Run query</button>
            </form>

            <div id="results"></div>

            <script>
                const queryId = {{JsonSerializer.Serialize(definition.Id)}};

                document.getElementById('query-form').addEventListener('submit', async (e) => {
                    e.preventDefault();
                    const params = new URLSearchParams(new FormData(e.target));

                    const resultsDiv = document.getElementById('results');
                    resultsDiv.innerHTML = '<p>Running…</p>';

                    try {
                        const res = await fetch(`/api/query/${queryId}/results?${params.toString()}`);
                        const html = await res.text();
                        if (!res.ok) {
                            resultsDiv.innerHTML = `<p class="error">${html}</p>`;
                            return;
                        }
                        resultsDiv.innerHTML = html;
                    } catch (err) {
                        resultsDiv.innerHTML = `<p class="error">${err.message}</p>`;
                    }
                });
            </script>
        </body>
        </html>
        """;
    }
}
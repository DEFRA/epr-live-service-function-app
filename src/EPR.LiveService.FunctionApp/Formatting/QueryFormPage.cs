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
            <script src="https://cdnjs.cloudflare.com/ajax/libs/list.js/2.3.1/list.min.js"></script>
            <style>
                body { font-family: system-ui, sans-serif; max-width: 900px; margin: 2rem auto; background: #1e1e1e; color: #d4d4d4; }
                label { display: block; margin-top: 0.75rem; font-weight: 600; }
                input[type="text"], input[type="number"], input[type="date"] {
                    width: 100%; padding: 0.4rem; margin-top: 0.25rem; box-sizing: border-box;
                }
                fieldset { margin-top: 1rem; border: 1px solid #3a3a3a; border-radius: 4px; padding: 0.75rem 1rem; }
                legend { font-weight: 600; padding: 0 0.4rem; }
                .radio-option { font-weight: normal; display: flex; align-items: center; gap: 0.4rem; margin-top: 0.4rem; }
                .radio-option input { width: auto; margin: 0; }
                .option-hint { color: #aaaaaa; font-weight: normal; margin-left: 0.3rem; }
                button { margin-top: 1.25rem; padding: 0.5rem 1.2rem; }
                #results { margin-top: 1.5rem; }

                .query-results-list { margin-top: 1rem; }
                .table-scroll { overflow-x: auto; max-width: 100%; }

                table { border-collapse: collapse; }
                th, td {
                    border: 1px solid #3a3a3a;
                    padding: 0.4rem 0.6rem;
                    text-align: left;
                    white-space: nowrap;
                }
                th { cursor: pointer; user-select: none; background: #2a2a2a; }
                th:hover { background: #333; }
                th.asc::after { content: ' ▲'; }
                th.desc::after { content: ' ▼'; }

                .error { color: #ff6b6b; }
            </style>
        </head>
        <body>
            <h1>{{WebUtility.HtmlEncode(definition.DisplayName)}}</h1>
            <p>{{WebUtility.HtmlEncode(definition.Description)}}</p>

            <form id="query-form">
                {{fields}}

                <fieldset>
                    <legend>Output format</legend>
                    <label class="radio-option">
                        <input type="radio" name="output" value="html" checked />
                        HTML Table
                        <span class="option-hint">— sortable by column</span>
                    </label>
                    <label class="radio-option">
                        <input type="radio" name="output" value="ascii_table" />
                        ASCII Table
                        <span class="option-hint">— good for copy-pasting, nicely formatted</span>
                    </label>
                    <label class="radio-option">
                        <input type="radio" name="output" value="csv" />
                        CSV
                        <span class="option-hint">— best for large result sets</span>
                    </label>
                </fieldset>

                <button type="submit">Run query</button>
            </form>

            <div id="results"></div>

            <script>
                const queryId = {{JsonSerializer.Serialize(definition.Id)}};

                document.getElementById('query-form').addEventListener('submit', async (e) => {
                    e.preventDefault();
                    const params = new URLSearchParams(new FormData(e.target));
                    const url = `/api/query/${queryId}/results?${params.toString()}`;

                    if (params.get('output') === 'csv') {
                        window.location.href = url;
                        return;
                    }

                    const resultsDiv = document.getElementById('results');
                    resultsDiv.innerHTML = '<p>Running…</p>';

                    try {
                        const res = await fetch(url);
                        const html = await res.text();
                        if (!res.ok) {
                            resultsDiv.innerHTML = `<p class="error">${html}</p>`;
                            return;
                        }
                        resultsDiv.innerHTML = html;

                        const container = document.getElementById('query-results-list');
                        if (container) {
                            const valueNames = Array.from(container.querySelectorAll('th[data-sort]'))
                                .map(th => th.dataset.sort);
                            new List('query-results-list', { valueNames });
                        }
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
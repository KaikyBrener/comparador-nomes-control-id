using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// FRONTEND
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang=""pt-BR"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Comparador Empresarial - Control ID</title>

    <!-- Estilos e Fontes CDNs -->
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet"" />
    <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;600;700&display=swap"" rel=""stylesheet"" />
    <script src=""https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/sweetalert2@11""></script>

    <!-- Estilos Custom -->
    <style>
      * { font-family: ""Poppins"", sans-serif; }
      body {
        background: linear-gradient(to top, #0a0f1a, #1e293b);
        color: #e5e7eb;
        font-size: 16px;
      }
      .card {
        background: #1e293b;
        border: 1px solid #334155;
        border-radius: 15px;
        box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3);
        transition: transform 0.3s, box-shadow 0.3s;
      }
      .card:hover { transform: translateY(-5px); box-shadow: 0 6px 25px rgba(0, 0, 0, 0.4); }
      .nav-tabs .nav-link {
        background: #334155; border: none; color: #e2e8f0; border-radius: 10px 10px 0 0;
      }
      .nav-tabs .nav-link.active { background: #2563eb; color: #ffffff; font-weight: bold; }
      textarea {
        resize: none; background: #334155; color: #e5e7eb;
        border-radius: 8px; border: 1px solid #3b82f6;
      }
      /* Botão com gradiente destacado */
      .btn-action {
        background: linear-gradient(135deg, #2563eb, #3b82f6);
        color: #fff; font-weight: 700; letter-spacing: 0.4px;
        border: none; border-radius: 10px;
        padding: 14px 36px;
        box-shadow: 0 10px 25px rgba(37, 99, 235, 0.35);
        transition: transform 0.2s ease, box-shadow 0.25s ease, filter 0.25s ease;
      }
      .btn-action:hover {
        transform: translateY(-2px);
        box-shadow: 0 14px 32px rgba(37, 99, 235, 0.45);
        filter: brightness(1.08);
      }
      .btn-action:active { transform: translateY(0); filter: brightness(0.95); }
    </style>
  </head>
  <body>
    <main class=""min-vh-100 d-flex flex-column justify-content-center align-items-center"">
      <div class=""container p-4"">
        <!-- Título -->
        <div class=""text-center mb-5"">
          <h1 class=""fw-bold display-4"" style=""color: #60a5fa;"">🔍 Comparador Empresarial</h1>
          <p class=""fs-5 text-secondary"">Detecta divergências e semelhanças entre listas de nomes com rapidez!</p>
        </div>
        <!-- Formulário -->
        <div class=""card p-5 mb-5"">
          <div class=""row g-4"">
            <div class=""col-md-6"">
              <label for=""sistema"" class=""form-label text-light"">Lista do Sistema</label>
              <textarea id=""sistema"" class=""form-control"" rows=""10"" placeholder=""Cole sua lista""></textarea>
              <input type=""file"" class=""form-control mt-2"" id=""fileSistema"" accept="".txt"">
            </div>
            <div class=""col-md-6"">
              <label for=""control"" class=""form-label text-light"">Lista do Control ID</label>
              <textarea id=""control"" class=""form-control"" rows=""10"" placeholder=""Cole sua lista""></textarea>
              <input type=""file"" class=""form-control mt-2"" id=""fileControl"" accept="".txt"">
            </div>
          </div>
          <div class=""text-center mt-4"">
            <button id=""btnComparar"" class=""btn-action"">Comparar 🔍</button>
          </div>
        </div>
        <!-- Resultados -->
        <div class=""card"">
          <ul class=""nav nav-tabs"" id=""resultTabs"">
            <li class=""nav-item"">
              <button class=""nav-link active"" data-bs-toggle=""tab"" data-bs-target=""#removerTab"">🚨 Para Remover</button>
            </li>
            <li class=""nav-item"">
              <button class=""nav-link"" data-bs-toggle=""tab"" data-bs-target=""#similaresTab"">🤝 Similaridades</button>
            </li>
          </ul>
          <div class=""tab-content p-4"">
            <div class=""tab-pane fade show active"" id=""removerTab"">
              <textarea id=""resultadoRemover"" class=""form-control"" rows=""10"" readonly></textarea>
            </div>
            <div class=""tab-pane fade"" id=""similaresTab"">
              <textarea id=""resultadoSimilares"" class=""form-control"" rows=""10"" readonly></textarea>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Bootstrap Bundle -->
    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js""></script>

    <script>
      document.getElementById(""btnComparar"").addEventListener(""click"", async () => {
        const fileSistema = document.getElementById(""fileSistema"").files[0];
        const fileControl = document.getElementById(""fileControl"").files[0];
        let sistema = document.getElementById(""sistema"").value;
        let control = document.getElementById(""control"").value;

        if (fileSistema) sistema = await fileSistema.text();
        if (fileControl) control = await fileControl.text();

        if (!sistema || !control) {
          Swal.fire(""Erro"", ""Preencha ou envie os dois lados"", ""error"");
          return;
        }

        try {
          const response = await axios.post(""http://localhost:5000/comparar"", { sistema, control });
          document.getElementById(""resultadoRemover"").value = (response.data.remover || []).join(""\n"");
          document.getElementById(""resultadoSimilares"").value = (response.data.similares || []).join(""\n"");
        } catch (error) {
          console.error(error);
          Swal.fire(""Erro"", ""Tivemos um problema ao processar"", ""error"");
        }
      });
    </script>
  </body>
</html>
", "text/html"));

// BACKEND
app.MapPost("/comparar", async (HttpContext context) =>
{
    var data = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();

    if (data == null || !data.ContainsKey("sistema") || !data.ContainsKey("control"))
        return Results.BadRequest("Dados inválidos");

    var sistema = data["sistema"]
        .Split('\n')
        .Select(n => n.Trim().ToLower())
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .ToHashSet();

    var control = data["control"]
        .Split('\n')
        .Select(n => n.Trim())
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .ToList();

    List<string> remover = new();
    List<string> similares = new();

    foreach (var nome in control)
    {
        var lower = nome.ToLower();
        if (sistema.Contains(lower))
            continue;

        // similaridade simples: substring em qualquer direção (case-insensitive)
        var similar = sistema.FirstOrDefault(s =>
            s.Contains(lower) || lower.Contains(s));

        if (!string.IsNullOrEmpty(similar))
            similares.Add($"{nome} ~ {similar}");
        else
            remover.Add(nome);
    }

    return Results.Json(new { remover, similares });
});

// IMPORTANTE: usar localhost
app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços
builder.Services.AddControllers();

// Configura CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

var app = builder.Build();

// Usa CORS
app.UseCors("AllowAll");

// Rotas padrão
app.MapControllers();

// LOG EM MEMÓRIA
List<Dictionary<string, object>> logComparacoes = new();

int CalcularSimilaridade(string nome1, string nome2)
{
    var s1 = nome1.ToLower().Trim();
    var s2 = nome2.ToLower().Trim();
    if (s1 == s2) return 100;
    if (s1.Contains(s2) || s2.Contains(s1)) return 80;
    return Math.Max(0, 100 - (Math.Max(s1.Length, s2.Length) - Math.Min(s1.Length, s2.Length)) * 20);
}

List<string> ExtrairNomes(string dados)
{
    var linhas = dados.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("|")).ToList();
    List<string> nomes = new();
    bool isPrimeiraLinha = true;

    foreach (var linha in linhas)
    {
        if (isPrimeiraLinha) { isPrimeiraLinha = false; if (linha.Contains("Nome") || linha.Contains("APTO") || linha.Contains("---")) continue; }
        if (linha.Replace("|", "").Replace("-", "").Replace(" ", "").Length == 0) continue;

        var partes = linha.Split('|').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        string nome = "";
        if (partes.Count >= 4 && !int.TryParse(partes[3], out _)) nome = partes[3];
        else if (partes.Count >= 3 && !int.TryParse(partes[2], out _)) nome = partes[2];
        else if (partes.Count >= 2 && !int.TryParse(partes[1], out _)) nome = partes[1];
        else if (partes.Count >= 1) nome = partes[0];
        if (!string.IsNullOrWhiteSpace(nome)) nomes.Add(nome);
    }
    return nomes;
}

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
    <script src=""https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js""></script>

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
        font-weight: 600;
        cursor: pointer;
        transition: all 0.3s;
      }
      .nav-tabs .nav-link:hover {
        background: #475569;
        color: #60a5fa;
      }
      .nav-tabs .nav-link.active { 
        background: linear-gradient(135deg, #2563eb, #3b82f6);
        color: #ffffff; 
        font-weight: bold;
        box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
      }
      textarea {
        resize: none; 
        background: #334155; 
        color: #e5e7eb;
        border-radius: 8px; 
        border: 1px solid #3b82f6;
        transition: all 0.3s;
      }
      textarea:focus {
        background: #334155;
        color: #e5e7eb;
        border-color: #60a5fa;
        box-shadow: 0 0 10px rgba(96, 165, 250, 0.3);
      }
      .form-control {
        background: #334155;
        border: 1px solid #3b82f6;
        color: #e5e7eb;
        border-radius: 8px;
      }
      .form-control:focus {
        background: #334155;
        border-color: #60a5fa;
        color: #e5e7eb;
        box-shadow: 0 0 10px rgba(96, 165, 250, 0.3);
      }
      .form-select {
        background: #334155 !important;
        border: 1px solid #3b82f6 !important;
        color: #e5e7eb !important;
        border-radius: 8px;
      }
      .form-select:focus {
        border-color: #60a5fa !important;
        box-shadow: 0 0 10px rgba(96, 165, 250, 0.3) !important;
      }
      .btn-action {
        background: linear-gradient(135deg, #2563eb, #3b82f6);
        color: #fff; 
        font-weight: 700; 
        letter-spacing: 0.4px;
        border: none; 
        border-radius: 10px;
        padding: 14px 36px;
        box-shadow: 0 10px 25px rgba(37, 99, 235, 0.35);
        transition: transform 0.2s ease, box-shadow 0.25s ease, filter 0.25s ease;
        cursor: pointer;
      }
      .btn-action:hover {
        transform: translateY(-2px);
        box-shadow: 0 14px 32px rgba(37, 99, 235, 0.45);
        filter: brightness(1.08);
      }
      .btn-action:active { 
        transform: translateY(0); 
        filter: brightness(0.95); 
      }
      .btn-secondary {
        background: linear-gradient(135deg, #475569, #64748b);
        padding: 10px 20px;
        margin: 5px;
        border: none;
        border-radius: 8px;
        color: #fff;
        cursor: pointer;
        font-weight: 600;
        transition: all 0.3s;
      }
      .btn-secondary:hover { 
        background: linear-gradient(135deg, #64748b, #94a3b8);
        transform: translateY(-2px);
        box-shadow: 0 6px 15px rgba(0, 0, 0, 0.3);
      }
      .score-100 { color: #22c55e; font-weight: bold; }
      .score-80 { color: #eab308; font-weight: bold; }
      .score-0 { color: #ef4444; font-weight: bold; }
      .form-label {
        color: #cbd5e1;
        font-weight: 600;
        margin-bottom: 8px;
      }
      .swal2-popup {
        background: #1e293b !important;
        color: #e5e7eb !important;
        border-radius: 15px !important;
        box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5) !important;
      }
      .swal2-title {
        color: #60a5fa !important;
        font-weight: 700 !important;
      }
      .swal2-html-container {
        color: #cbd5e1 !important;
      }
      .swal2-confirm {
        background: linear-gradient(135deg, #2563eb, #3b82f6) !important;
        border-radius: 8px !important;
        font-weight: 600 !important;
      }
      .swal2-confirm:hover {
        background: linear-gradient(135deg, #1d4ed8, #2563eb) !important;
      }
      .swal2-cancel {
        background: #475569 !important;
        border-radius: 8px !important;
        font-weight: 600 !important;
      }
      .swal2-cancel:hover {
        background: #64748b !important;
      }
    </style>
  </head>
  <body>
    <main class=""min-vh-100 d-flex flex-column justify-content-center align-items-center"">
      <div class=""container p-4"">
        <div class=""text-center mb-5"">
          <h1 class=""fw-bold display-4"" style=""color: #60a5fa;"">🔍 Comparador De Nomes</h1>
          <p class=""fs-5 text-secondary"">Detecta divergências e semelhanças entre listas de nomes com rapidez!</p>
        </div>

        <!-- ABAS PRINCIPAIS -->
        <ul class=""nav nav-tabs mb-4"" role=""tablist"">
          <li class=""nav-item"">
            <button class=""nav-link active"" id=""abaModoTab"" data-bs-toggle=""tab"" data-bs-target=""#abaModo"">🔍 Comparador</button>
          </li>
          <li class=""nav-item"">
            <button class=""nav-link"" id=""abaLimpezaTab"" data-bs-toggle=""tab"" data-bs-target=""#abaLimpeza"">🧹 Limpeza</button>
          </li>
          <li class=""nav-item"">
            <button class=""nav-link"" id=""abaLogTab"" data-bs-toggle=""tab"" data-bs-target=""#abaLog"">📋 Log</button>
          </li>
        </ul>

        <div class=""tab-content"">
          <!-- ABA COMPARADOR -->
          <div class=""tab-pane fade show active"" id=""abaModo"">
            <div class=""card p-5 mb-5"">
              <div class=""row g-4"">
                <div class=""col-md-6"">
                  <label for=""sistema"" class=""form-label"">Lista do Sistema</label>
                  <textarea id=""sistema"" class=""form-control"" rows=""10"" placeholder=""Cole sua lista""></textarea>
                  <input type=""file"" class=""form-control mt-2"" id=""fileSistema"" accept="".txt"">
                </div>
                <div class=""col-md-6"">
                  <label for=""control"" class=""form-label"">Lista do Control ID</label>
                  <textarea id=""control"" class=""form-control"" rows=""10"" placeholder=""Cole sua lista""></textarea>
                  <input type=""file"" class=""form-control mt-2"" id=""fileControl"" accept="".txt"">
                </div>
              </div>
              <div class=""text-center mt-4"">
                <button id=""btnComparar"" class=""btn-action"">Comparar 🔍</button>
              </div>
            </div>
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

          <!-- ABA LIMPEZA -->
          <div class=""tab-pane fade"" id=""abaLimpeza"">
            <div class=""card p-5"">
              <div class=""mb-3"">
                <label for=""tipoFabricante"" class=""form-label"">Selecione a Fonte de Dados</label>
                <select id=""tipoFabricante"" class=""form-select"">
                  <option value=""sistema"">Sistema (Nome)</option>
                  <option value=""hikvision"">Hikvision (Person ID + Name)</option>
                  <option value=""control"">Control ID (Nome)</option>
                  <option value=""intelbras"">Intelbras (Nome)</option>
                </select>
              </div>
              <div class=""mb-3"">
                <label for=""dadosLimpeza"" class=""form-label"">Cole os dados brutos</label>
                <textarea id=""dadosLimpeza"" class=""form-control"" rows=""12"" placeholder=""Cole os dados aqui...""></textarea>
              </div>
              <div class=""text-center mb-3"">
                <button id=""btnLimpar"" class=""btn-action"">Limpar 🧹</button>
              </div>
              <div class=""mb-3"">
                <label for=""resultadoLimpeza"" class=""form-label"">Resultado</label>
                <textarea id=""resultadoLimpeza"" class=""form-control"" rows=""12"" readonly placeholder=""Dados limpos aparecerão aqui...""></textarea>
              </div>
              <div class=""text-center"">
                <button id=""btnCopiar"" class=""btn-secondary"">📋 Copiar</button>
              </div>
            </div>
          </div>

          <!-- ABA LOG -->
          <div class=""tab-pane fade"" id=""abaLog"">
            <div class=""card p-5"">
              <div class=""d-flex justify-content-between align-items-center mb-4"">
                <h5 class=""text-light mb-0"">📋 Histórico de Comparações</h5>
                <div>
                  <button id=""btnBaixarLog"" class=""btn-secondary"">⬇️ Download TXT</button>
                  <button id=""btnLimparLog"" class=""btn-secondary"">🗑️ Limpar Log</button>
                </div>
              </div>
              <div style=""overflow-x: auto;"">
                <table class=""table table-dark table-striped"">
                  <thead>
                    <tr>
                      <th>Nome</th>
                      <th>Ação</th>
                      <th>Score</th>
                      <th>Data/Hora</th>
                    </tr>
                  </thead>
                  <tbody id=""corpoLog"">
                    <tr><td colspan=""4"" class=""text-center text-secondary"">Nenhum registro ainda</td></tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js""></script>
    <script>
      function apiUrl(path) {
        // Usa a mesma origem da página (funciona em localhost e produção)
        return window.location.origin + path;
      }

      function atualizarTabelaLog() {
        axios.get(apiUrl('/log')).then(response => {
          const logs = response.data.logs || [];
          const tbody = document.getElementById('corpoLog');
          
          if (logs.length === 0) {
            tbody.innerHTML = '<tr><td colspan=""4"" class=""text-center text-secondary"">Nenhum registro ainda</td></tr>';
            return;
          }

          tbody.innerHTML = logs.map(log => {
            const scoreClass = log.score === 100 ? 'score-100' : (log.score === 80 ? 'score-80' : 'score-0');
            return '<tr><td>' + log.nome + '</td><td>' + log.acao + '</td><td><span class=' + scoreClass + '>' + log.score + '%</span></td><td>' + log.timestamp + '</td></tr>';
          }).join('');
        });
      }

      // COMPARADOR
      document.getElementById(""btnComparar"").addEventListener(""click"", async () => {
        const fileSistema = document.getElementById(""fileSistema"").files[0];
        const fileControl = document.getElementById(""fileControl"").files[0];
        let sistema = document.getElementById(""sistema"").value;
        let control = document.getElementById(""control"").value;

        if (fileSistema) sistema = await fileSistema.text();
        if (fileControl) control = await fileControl.text();

        if (!sistema || !control) {
          Swal.fire({
            icon: 'warning',
            title: 'Dados Incompletos',
            html: 'Por favor, preencha ou envie os dois lados (Sistema e Control ID)',
            confirmButtonText: 'Entendi',
            allowOutsideClick: false
          });
          return;
        }

        Swal.fire({
          title: 'Processando...',
          html: 'Comparando as listas, aguarde um momento',
          icon: 'info',
          allowOutsideClick: false,
          didOpen: () => {
            Swal.showLoading();
          }
        });

        try {
          const url = apiUrl('/comparar');
          const response = await axios.post(url, { sistema, control });
          document.getElementById(""resultadoRemover"").value = (response.data.remover || []).join(""\n"");
          document.getElementById(""resultadoSimilares"").value = (response.data.similares || []).join(""\n"");
          atualizarTabelaLog();

          Swal.fire({
            icon: 'success',
            title: 'Comparação Concluída!',
            html: '<div style=""text-align: left;""><p><strong>Removidos:</strong> ' + response.data.remover.length + '</p><p><strong>Similares:</strong> ' + response.data.similares.length + '</p></div>',
            confirmButtonText: 'Ver Resultados',
            allowOutsideClick: false
          });
        } catch (error) {
          Swal.fire({
            icon: 'error',
            title: 'Erro ao Processar',
            text: error.message || 'Houve um problema ao comparar as listas',
            confirmButtonText: 'Fechar',
            allowOutsideClick: false
          });
        }
      });

      // LIMPEZA
      document.getElementById(""btnLimpar"").addEventListener(""click"", async () => {
        const dados = document.getElementById(""dadosLimpeza"").value;
        const tipo = document.getElementById(""tipoFabricante"").value;

        if (!dados.trim()) {
          Swal.fire({
            icon: 'warning',
            title: 'Dados Vazios',
            text: 'Por favor, cole os dados que deseja limpar',
            confirmButtonText: 'Ok',
            allowOutsideClick: false
          });
          return;
        }

        Swal.fire({
          title: 'Limpando Dados...',
          html: 'Processando os dados, aguarde',
          icon: 'info',
          allowOutsideClick: false,
          didOpen: () => {
            Swal.showLoading();
          }
        });

        try {
          const url = apiUrl('/limpar');
          const response = await axios.post(url, { dados, tipo });
          
          if (!response.data.limpo || response.data.limpo.trim() === '') {
            Swal.fire({
              icon: 'warning',
              title: 'Nenhum Dado Encontrado',
              text: 'Nenhum dado válido foi encontrado para este formato. Verifique o formato dos dados.',
              confirmButtonText: 'Entendi',
              allowOutsideClick: false
            });
            document.getElementById(""resultadoLimpeza"").value = '';
            return;
          }
          
          document.getElementById(""resultadoLimpeza"").value = response.data.limpo;
          
          Swal.fire({
            icon: 'success',
            title: 'Dados Limpos!',
            html: '<div style=""text-align: left;""><p><strong>Linhas processadas:</strong> ' + response.data.limpo.split('\n').length + '</p><p style=""color: #60a5fa; margin-top: 10px;"">Os dados estão prontos para copiar!</p></div>',
            confirmButtonText: 'Fechar',
            allowOutsideClick: false
          });
        } catch (error) {
          Swal.fire({
            icon: 'error',
            title: 'Erro ao Processar',
            text: error.message || 'Houve um problema ao limpar os dados',
            confirmButtonText: 'Fechar',
            allowOutsideClick: false
          });
        }
      });

      // COPIAR
      document.getElementById(""btnCopiar"").addEventListener(""click"", () => {
        const resultado = document.getElementById(""resultadoLimpeza"");
        if (!resultado.value) {
          Swal.fire({
            icon: 'info',
            title: 'Nada para Copiar',
            text: 'Realize a limpeza de dados primeiro',
            confirmButtonText: 'Ok',
            allowOutsideClick: false
          });
          return;
        }
        navigator.clipboard.writeText(resultado.value);
        Swal.fire({
          icon: 'success',
          title: 'Copiado!',
          text: 'Dados copiados para a área de transferência',
          timer: 1500,
          timerProgressBar: true,
          showConfirmButton: false
        });
      });

      // DOWNLOAD LOG
      document.getElementById(""btnBaixarLog"").addEventListener(""click"", async () => {
        try {
          Swal.fire({
            title: 'Preparando Download...',
            icon: 'info',
            allowOutsideClick: false,
            didOpen: () => {
              Swal.showLoading();
            }
          });

          const response = await axios.get(apiUrl('/log/download'), { responseType: 'text' });
          const element = document.createElement('a');
          element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(response.data));
          element.setAttribute('download', 'log_comparacoes.txt');
          element.style.display = 'none';
          document.body.appendChild(element);
          element.click();
          document.body.removeChild(element);
          
          Swal.fire({
            icon: 'success',
            title: 'Download Iniciado!',
            text: 'Arquivo log_comparacoes.txt foi baixado',
            timer: 1500,
            timerProgressBar: true,
            showConfirmButton: false
          });
        } catch (error) {
          Swal.fire({
            icon: 'error',
            title: 'Erro no Download',
            text: error.message,
            confirmButtonText: 'Fechar',
            allowOutsideClick: false
          });
        }
      });

      // LIMPAR LOG
      document.getElementById(""btnLimparLog"").addEventListener(""click"", async () => {
        Swal.fire({
          icon: 'warning',
          title: 'Limpar Log?',
          text: 'Esta ação não pode ser desfeita. Deseja continuar?',
          showCancelButton: true,
          confirmButtonText: 'Sim, Limpar',
          cancelButtonText: 'Cancelar',
          allowOutsideClick: false
        }).then((result) => {
          if (result.isConfirmed) {
            axios.post(apiUrl('/log/limpar')).then(() => {
              Swal.fire({
                icon: 'success',
                title: 'Log Limpo!',
                text: 'O histórico foi apagado com sucesso',
                timer: 1500,
                timerProgressBar: true,
                showConfirmButton: false
              });
              atualizarTabelaLog();
            }).catch((error) => {
              Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: error.message,
                confirmButtonText: 'Fechar',
                allowOutsideClick: false
              });
            });
          }
        });
      });

      // Atualiza log quando muda de aba
      document.getElementById(""abaLogTab"").addEventListener(""shown.bs.tab"", atualizarTabelaLog);
    </script>
  </body>
</html>
", "text/html"));

// BACKEND - COMPARADOR
app.MapPost("/comparar", async (HttpContext context) =>
{
    try
    {
        var data = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();

        if (data == null || !data.ContainsKey("sistema") || !data.ContainsKey("control"))
            return Results.BadRequest("Dados inválidos");

        var sistemaNomes = ExtrairNomes(data["sistema"]);
        var controlNomes = ExtrairNomes(data["control"]);

        var sistemaNormalizadas = sistemaNomes
            .Select(n => n.ToLower().Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet();

        List<string> remover = new();
        List<string> similares = new();

        foreach (var nome in controlNomes)
        {
            var lower = nome.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(lower)) continue;

            if (sistemaNormalizadas.Contains(lower)) continue;

            var similar = sistemaNormalizadas.FirstOrDefault(s =>
                s.Contains(lower) || lower.Contains(s));

            if (!string.IsNullOrEmpty(similar))
            {
                similares.Add(nome);
                int score = CalcularSimilaridade(nome, similar);
                logComparacoes.Add(new Dictionary<string, object>
                {
                    { "nome", nome },
                    { "acao", "Similar" },
                    { "score", score },
                    { "timestamp", DateTime.Now.ToString("dd/MM HH:mm:ss") }
                });
            }
            else
            {
                remover.Add(nome);
                logComparacoes.Add(new Dictionary<string, object>
                {
                    { "nome", nome },
                    { "acao", "Remover" },
                    { "score", 0 },
                    { "timestamp", DateTime.Now.ToString("dd/MM HH:mm:ss") }
                });
            }
        }

        return Results.Json(new { remover, similares });
    }
    catch (System.Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

// BACKEND - GET LOG
app.MapGet("/log", () =>
{
    var logsOrdenados = logComparacoes.OrderByDescending(l => l["timestamp"]).ToList();
    return Results.Json(new { logs = logsOrdenados });
});

// BACKEND - DOWNLOAD LOG
app.MapGet("/log/download", () =>
{
    var linhas = new List<string> { "HISTORICO DE COMPARACOES", "========================", "" };
    foreach (var log in logComparacoes.OrderByDescending(l => l["timestamp"]))
    {
        linhas.Add(log["nome"] + " | " + log["acao"] + " | " + log["score"] + "% | " + log["timestamp"]);
    }
    return Results.Text(string.Join("\n", linhas));
});

// BACKEND - LIMPAR LOG
app.MapPost("/log/limpar", () =>
{
    logComparacoes.Clear();
    return Results.Json(new { message = "Log limpo" });
});

// BACKEND - LIMPEZA
app.MapPost("/limpar", async (HttpContext context) =>
{
    try
    {
        var data = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
        if (data == null || !data.ContainsKey("dados") || !data.ContainsKey("tipo"))
            return Results.BadRequest("Dados inválidos");

        var dados = data["dados"];
        var tipo = data["tipo"].ToLower();

        var linhas = dados.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("|") && !l.Contains("---"))
            .ToList();

        List<string> resultado = new();
        HashSet<string> vistos = new();
        bool isPrimeiraLinha = true;

        foreach (var linha in linhas)
        {
            if (linha.Replace("|", "").Replace("-", "").Replace(" ", "").Length == 0)
                continue;

            if (isPrimeiraLinha)
            {
                isPrimeiraLinha = false;
                if (linha.Contains("Person ID") || linha.Contains("Nome") || linha.Contains("Codigo") ||
                    linha.Contains("User Type") || linha.Contains("Tipo") || linha.Contains("Estado") ||
                    linha.Contains("Departamento") || linha.Contains("Name") || linha.Contains("User") ||
                    linha.Contains("Floor") || linha.Contains("Matricula") || linha.Contains("APTO"))
                    continue;
            }

            string processada = "";

            var partes = linha.Contains("|")
                ? linha.Split('|').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList()
                : Regex.Split(linha, @"\s{2,}|\t").Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            if (partes.Count == 0) continue;

            if (tipo == "sistema")
            {
                if (partes.Count >= 4)
                {
                    var nome = partes[3];
                    if (!string.IsNullOrWhiteSpace(nome))
                        processada = nome;
                }
            }
            else if (tipo == "hikvision")
            {
                if (partes.Count >= 3 && int.TryParse(partes[0], out _))
                {
                    var personId = partes[0];
                    var name = partes[2];
                    processada = personId + " | " + name;
                }
            }
            else if (tipo == "control")
            {
                if (partes.Count >= 2)
                {
                    var nome = partes[1];
                    if (!string.IsNullOrWhiteSpace(nome))
                        processada = nome;
                }
            }
            else if (tipo == "intelbras")
            {
                if (partes.Count >= 1)
                {
                    var nome = partes[0];
                    if (!string.IsNullOrWhiteSpace(nome))
                        processada = nome;
                }
            }

            if (!string.IsNullOrWhiteSpace(processada) && !vistos.Contains(processada.ToLower()))
            {
                resultado.Add(processada);
                vistos.Add(processada.ToLower());
            }
        }

        return Results.Json(new { limpo = string.Join("\n", resultado) });
    }
    catch (System.Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.Run();
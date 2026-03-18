var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// FRONTEND
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
<meta charset='UTF-8'>
<title>Comparador de Nomes</title>
<link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css' rel='stylesheet' />
<script src='https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js'></script>
<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
</head>

<body class='bg-dark text-light'>
<div class='container py-5'>
    <div class='card shadow-lg'>
        <div class='card-body bg-dark'>
            <h2 class='text-center mb-4'>🔍 Comparador de Nomes</h2>
            
            <div class='row g-4'>
                <div class='col-md-6'>
                    <label class='form-label'>Sistema</label>
                    <textarea id='sistema' class='form-control' rows='10' placeholder='Cole a lista do sistema'></textarea>
                    <input type='file' id='fileSistema' class='form-control mt-2' accept='.txt'>
                </div>
                
                <div class='col-md-6'>
                    <label class='form-label'>Control ID</label>
                    <textarea id='control' class='form-control' rows='10' placeholder='Cole a lista do Control ID'></textarea>
                    <input type='file' id='fileControl' class='form-control mt-2' accept='.txt'>
                </div>
            </div>
            
            <div class='text-center mt-4'>
                <button class='btn btn-success btn-lg' onclick='comparar()'>Comparar</button>
            </div>
        </div>
    </div>
</div>

<script>
async function lerArquivo(file) {
    if (!file) return '';
    return await file.text();
}

async function comparar() {
    let sistema = document.getElementById('sistema').value;
    let control = document.getElementById('control').value;

    const fileSistema = document.getElementById('fileSistema').files[0];
    const fileControl = document.getElementById('fileControl').files[0];

    if (fileSistema) sistema = await lerArquivo(fileSistema);
    if (fileControl) control = await lerArquivo(fileControl);

    if (!sistema || !control) {
        Swal.fire('Erro', 'Preencha ou envie os dois lados!', 'error');
        return;
    }

    try {
        const response = await axios.post('/comparar', { sistema, control });
        mostrarResultado(response.data);
    } catch (e) {
        Swal.fire('Erro', 'Falha ao comparar', 'error');
    }
}

function mostrarResultado(lista) {
    let texto = lista.join('\\n');
    Swal.fire({
        title: '🚨 Nomes para remover',
        html: `
            <textarea id='resultado' class='form-control' rows='10'>${texto}</textarea>
            <div class='mt-3'>
                <button class='btn btn-primary me-2' onclick='copiar()'>Copiar</button>
                <button class='btn btn-warning' onclick='baixar()'>Baixar</button>
            </div>
        `,
        width: 600,
        showConfirmButton: false
    });
}

function copiar() {
    let el = document.getElementById('resultado');
    el.select();
    document.execCommand('copy');
    Swal.fire('Copiado!', '', 'success');
}

function baixar() {
    let texto = document.getElementById('resultado').value;
    let blob = new Blob([texto], { type: 'text/plain' });
    let link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'remover.txt';
    link.click();
}
</script>
</body>
</html>
", "text/html"));

// BACKEND
app.MapPost("/comparar", async (HttpContext context) =>
{
    var data = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();

    var sistema = data["sistema"]
        .Split('\n')
        .Select(n => n.Trim().ToLower())
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .ToHashSet();

    var control = data["control"]
        .Split('\n')
        .Select(n => n.Trim())
        .Where(n => !string.IsNullOrWhiteSpace(n));

    var remover = control
        .Where(nome => !sistema.Contains(nome.ToLower()))
        .Distinct()
        .ToList();

    return Results.Json(remover);
});

app.Run("http://0.0.0.0:5000");

using PruebaCorta2;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ChairDb>(opt => opt.UseInMemoryDatabase("Chair-List"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.MapGet("/chair", async (ChairDb chairDb) =>
    await chairDb.Sillas.ToListAsync());

app.MapGet("/chair/{name}", async (string name, ChairDb chairDb) =>
    await chairDb.Sillas.Where(s => s.Nombre == name).FirstOrDefaultAsync()
        is Chair Sillas
            ? Results.Ok(Sillas)
            : Results.NotFound()
);

app.MapPost("/chair", async (ChairDb chairDb, Chair Silla) =>
{
    chairDb.Sillas.Add(Silla);
    await chairDb.SaveChangesAsync();
    return Results.Created($"/chair/{Silla.Id}", Silla);
});

app.MapPut("/chair/{id}", async (int id, ChairDb db, Chair chair) =>
{
    var existingChair = await db.Sillas.FindAsync(id);
    if (existingChair is null)
    {
        return Results.NotFound();
    }
    if (existingChair.Nombre != string.Empty)
    {
        existingChair.Nombre = chair.Nombre;
    }
    if (existingChair.Tipo != string.Empty)
    {
        existingChair.Tipo = chair.Tipo;
    }
    if (existingChair.Material != string.Empty)
    {
        existingChair.Material = chair.Material;
    }
    if (existingChair.Altura > 0)
    {
        existingChair.Altura = chair.Altura;
    }
    if (existingChair.Peso > 0)
    {
        existingChair.Peso = chair.Peso;
    }
    if (existingChair.Profundidad > 0)
    {
        existingChair.Profundidad = chair.Profundidad;
    }
    if (existingChair.Precio > 0)
    {
        existingChair.Precio = chair.Precio;
    }
    if (existingChair.Stock > 0)
    {
        existingChair.Stock = chair.Stock;
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPut("/chair/{id}/stock", async (int id, UpdateStockDto updateStockDto, ChairDb db) => 
{
    var existingChair = await db.Sillas.Where(c => c.Id == id).FirstOrDefaultAsync();
    if (existingChair == null)
    {
        return Results.NotFound();
    }
    if (updateStockDto.Stock <= 0)
    {
        return TypedResults.BadRequest("El stock a ingresar debe ser mayor a 0");
    }
    existingChair.Stock += updateStockDto.Stock;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/chair/{id}", async (int id, ChairDb db) =>
{
    if(await db.Sillas.FindAsync(id) is Chair chair)
    {
        db.Sillas.Remove(chair);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.MapPost("/chair/purchase", async (ChairDb db, PurchaseChairDto purchaseChairDto) => 
{
    var existingChair = await db.Sillas.Where(c => c.Id == purchaseChairDto.Id).FirstOrDefaultAsync();
    if(existingChair == null)
    {
        return Results.NotFound();
    }
    if(purchaseChairDto.Cantidad <= 0)
    {
        return TypedResults.BadRequest("Debe proporcionar los datos de la compra");
    }
    if(existingChair.Stock < purchaseChairDto.Cantidad)
    {
        return TypedResults.BadRequest("No hay suficiente stock para realizar la compra");
    }

    int total = existingChair.Precio * purchaseChairDto.Cantidad;
    if(purchaseChairDto.Pago != total)
    {
        return TypedResults.BadRequest("El pago no es válido");
    }
    existingChair.Stock -= purchaseChairDto.Cantidad;
    await db.SaveChangesAsync();
    return TypedResults.Ok("Se ha realizado la venta con éxito!");

});


app.Run();
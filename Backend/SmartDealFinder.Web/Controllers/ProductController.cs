using Microsoft.AspNetCore.Mvc;
using SmartDealFinder.Application.DTOs;
using SmartDealFinder.Application.Interfaces;
using SmartDealFinder.Domain.Entities;
using SmartDealFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartDealFinder.Web.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Product
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Merchant)
            .OrderBy(p => p.Title)
            .ToListAsync();
        return View(products);
    }

    // GET: Product/Create
    public IActionResult Create()
    {
        var categories = _context.Categories.OrderBy(c => c.Name).ToList();
        var merchants = _context.Merchants.OrderBy(m => m.Name).ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Merchants = new SelectList(merchants, "Id", "Name");
        return View();
    }

    // POST: Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        var categories = _context.Categories.OrderBy(c => c.Name).ToList();
        var merchants = _context.Merchants.OrderBy(m => m.Name).ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Merchants = new SelectList(merchants, "Id", "Name");
        return View(product);
    }

    // GET: Product/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Merchant)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound();

        var categories = _context.Categories.OrderBy(c => c.Name).ToList();
        var merchants = _context.Merchants.OrderBy(m => m.Name).ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Merchants = new SelectList(merchants, "Id", "Name");
        return View(product);
    }

    // POST: Product/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
            return BadRequest();

        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        var categories = _context.Categories.OrderBy(c => c.Name).ToList();
        var merchants = _context.Merchants.OrderBy(m => m.Name).ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Merchants = new SelectList(merchants, "Id", "Name");
        return View(product);
    }

    // GET: Product/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Merchant)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    // POST: Product/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Merchant)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound();
        return View(product);
    }
}
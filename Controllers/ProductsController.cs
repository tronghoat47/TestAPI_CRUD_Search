using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Practice_1.Models;
using System.Data;

namespace Practice_1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SE1631_DBContext _context;
        private readonly IMapper _mapper;
        public ProductsController(SE1631_DBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Get list products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            if (_context.Products == null)
            {
                return NotFound();
            }

            var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();

            var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);

            return Ok(productDTOs);
        }

        //Sort product
        [HttpGet("SortProduct")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> SortProduct([FromQuery] string sort, [FromQuery] string order)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }

            var sqlQuery = $"SELECT * FROM Products ORDER BY {sort} {order}";
            IEnumerable<Product> products;
            try
            {
                products = await _context.Products
                .FromSqlRaw(sqlQuery)
                //.Include(p => p.Category)
                .ToListAsync();
            }
            catch (Exception)
            {
                return BadRequest("Check param again");
            }
            

            var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);

            return Ok(productDTOs);
        }

        //Get product by ProductId
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.Include(p => p.Category).SingleOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var productDTO = _mapper.Map<ProductDTO>(product);

            return productDTO;
        }

        //Search products by CategoryId
        [HttpGet]
        [Route("Category/{CategoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> SearchProductsByCategoryId(int CategoryId)
        {
            if(_context.Products == null)
            {
                return NotFound();
            }

            var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == CategoryId)
            .ToListAsync();

            if(products.Count == 0)
            {
                return NoContent();
            }

            var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);

            return Ok(productDTOs);
        }

        //Add a new product
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] ProductDTO productDTO)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.SingleOrDefaultAsync(c =>productDTO.CategoryId == c.CategoryId);
            if (category == null)
            {
                return BadRequest("Don't have this category");
            }

            var product = _mapper.Map<Product>(productDTO);

            _context.Add(product);
            await _context.SaveChangesAsync();

            return Ok("Add successfully");
        }

        //Update product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductDTO productDTO, int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }

            if(!isProductExist(id))
            {
                return NotFound("Don't have this product");
            }

            var category = await _context.Categories.SingleOrDefaultAsync(c => productDTO.CategoryId == c.CategoryId);
            if (category == null)
            {
                return BadRequest("Don't have this category");
            }

            var product = _mapper.Map<Product>(productDTO);
            product.ProductId = id;

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Update successfully");
        }

        //Delete product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if(_context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }

            var hasOrderDetails = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            if(hasOrderDetails)
            {
                return Problem("Cannot delete the product");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool isProductExist(int id)
        {
            return (_context.Products?.Any(p => p.ProductId == id)).GetValueOrDefault();
        }
    }
}

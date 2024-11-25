using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebProject.DataConnection;
using WebProject.Models;

namespace WebProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ReviewsController(ApplicationDBContext context)
        {
            _context = context;
        }
        // API: Thêm đánh giá mới
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] Reviews review)
        {
            if (review == null || review.rating < 1 || review.rating > 5)
            {
                return BadRequest("Invalid review data. Rating must be between 1 and 5.");
            }

            var productExists = await _context.Products.AnyAsync(p => p.product_id == review.product_id);
            var userExists = await _context.Users.AnyAsync(u => u.user_id == review.user_id);

            if (!productExists || !userExists)
            {
                return NotFound("Product or User does not exist.");
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviewById), new { id = review.id }, review);
        }

        // API: Lấy đánh giá theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }
            return Ok(review);
        }

        // API: Lấy tất cả đánh giá cho một sản phẩm
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsByProduct(int productId)
        {
            var reviews = await _context.Reviews
                                        .Where(r => r.product_id == productId)
                                        .ToListAsync();

            if (reviews == null || reviews.Count == 0)
            {
                return NotFound($"No reviews found for product ID {productId}.");
            }
            return Ok(reviews);
        }

        // API: Lấy tất cả đánh giá của một người dùng
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviewsByUser(int userId)
        {
            var reviews = await _context.Reviews
                                        .Where(r => r.user_id == userId)
                                        .ToListAsync();

            if (reviews == null || reviews.Count == 0)
            {
                return NotFound($"No reviews found for user ID {userId}.");
            }
            return Ok(reviews);
        }

        // API: Cập nhật đánh giá
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] Reviews updatedReview)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }

            review.content = updatedReview.content ?? review.content;
            review.rating = updatedReview.rating;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }

        // API: Xóa đánh giá
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

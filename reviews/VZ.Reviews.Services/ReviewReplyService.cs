using Dawn;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VZ.Reviews.Services.Models.Data;
using VZ.Reviews.Services.Repositories;

namespace VZ.Reviews.Services
{
    public class ReviewReplyService
    {
        readonly IReviewReplyRepository _reviewReplyRepository;
        readonly ILogger _logger;

        public ReviewReplyService(ILogger logger)
        {
            _logger = logger;
            _reviewReplyRepository = new ReviewReplyRepository();
        }

        public async Task<ReviewReply> AddReviewReply(ReviewReply newReply)
        {
            Guard.Argument<string>(newReply.reviewId).NotNull().NotEmpty().NotWhiteSpace();
            await _reviewReplyRepository.AddAsync(newReply);
            return newReply;
        }

        public async Task<List<ReviewReply>> GetReviewReplies(string reviewId)
        {
            var parameters = new ValueTuple<string, object>[]
            {
                ("@rid", reviewId)
            };
            return await _reviewReplyRepository.GetSomeAsync<ReviewReply>("*", $"{nameof(ReviewReply.reviewId)} = @rid", parameters);
        }
    }
}

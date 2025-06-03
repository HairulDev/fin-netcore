using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Comment;
using api.Models;

namespace api.Mappers
{
    public static class CommentMapper
    {
        public static CommentDto ToCommentDto(this Comment commentModel)
        {
            return new CommentDto
            {
                Id = commentModel.Id,
                Title = commentModel.Title,
                Content = commentModel.Content,
                CreatedOn = commentModel.CreatedOn,
                FilePaths = string.IsNullOrEmpty(commentModel.FilePath) 
                    ? new List<string>() 
                    : commentModel.FilePath.Split(",").ToList(),
                CreatedBy = commentModel.AppUser?.UserName,
                StockId = commentModel.StockId
            };
        }

        public static Comment ToCommentFromCreate(this CreateCommentDto commentDto, int stockId)
        {
            return new Comment
            {
                Title = commentDto.Title,
                Content = commentDto.Content,
                FilePath = commentDto.FilePath,
                StockId = stockId
            };
        }

        public static Comment ToCommentFromUpdate(this UpdateCommentRequestDto commentDto, int stockId)
        {
            Console.WriteLine($"commentDto.FilePath received: {System.Text.Json.JsonSerializer.Serialize(commentDto.FilePath)}");
            return new Comment
            {
                Title = commentDto.Title,
                Content = commentDto.Content,
                FilePath = commentDto.FilePath,
                StockId = stockId
            };
        }

    }
}
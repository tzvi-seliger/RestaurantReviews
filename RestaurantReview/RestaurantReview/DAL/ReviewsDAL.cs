﻿using RestaurantReview.Models;
using RestaurantReview.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RestaurantReview.DAL
{
    public class ReviewsDAL
    {
        private readonly string connectionstring;

        public ReviewsDAL(string connString)
        {
            this.connectionstring = new Conn().AWSconnstring();
        }

        public List<GetReview> GetAllReviews()
        {
            List<GetReview> reviews = new List<GetReview>();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                conn.Open();
                SqlCommand SelectAll = new SqlCommand("SELECT * FROM Reviews " +
                    "JOIN Restaurants on Reviews.RestaurantId = Restaurants.RestaurantId " +
                    "JOIN Users on Reviews.UserId = Users.UserId", conn);
                SqlDataReader reader = SelectAll.ExecuteReader();
                while (reader.Read())
                {
                    reviews.Add(new GetReview
                    {
                        Restaurant = new Restaurant
                        {
                            RestaurantId = Convert.ToInt32(reader["RestaurantId"]),
                            City = Convert.ToString(reader["City"]),
                            Name = Convert.ToString(reader["Name"])
                        },
                        ReviewId = Convert.ToInt32(reader["ReviewId"]),
                        ReviewText = Convert.ToString(reader["ReviewText"]),
                        User = new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            UserName = Convert.ToString(reader["UserName"])
                        }
                    });
                }
            }
            return reviews;
        }

        public (bool IsSuccessful, PostReview toreturn) PostReview(PostReview review)
        {
            PostReview toreturn = new PostReview();
            bool IsSuccessful;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    toreturn.RestaurantId = review.RestaurantId;
                    toreturn.ReviewId = review.ReviewId;
                    toreturn.ReviewText = review.ReviewText;

                    //if (!review.ValidateUserNameFormat()) throw new HttpResponseException(HttpStatusCode.NotModified);
                    conn.Open();
                    SqlCommand SelectAll = new SqlCommand($"INSERT INTO Reviews VALUES(@RestaurantId, @UserId, @ReviewText);", conn);
                    SelectAll.Parameters.AddWithValue("@RestaurantId", review.RestaurantId);
                    SelectAll.Parameters.AddWithValue("@UserId", review.UserId);
                    SelectAll.Parameters.AddWithValue("@ReviewText", review.ReviewText);

                    SelectAll.ExecuteNonQuery();
                    IsSuccessful = true;
                }
            }
            catch
            {
                IsSuccessful = false;
            }
            return (IsSuccessful, toreturn);
        }

        public (bool IsSuccessful, UpdateReview toreturn) UpdateReview(UpdateReview updateReview)
        {
            UpdateReview toreturn = new UpdateReview();
            bool IsSuccessful;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    conn.Open();
                    SqlCommand updateReviewCmd = new SqlCommand($"UPDATE Reviews SET ReviewText = @reviewText WHERE ReviewId = @id;", conn);
                    updateReviewCmd.Parameters.AddWithValue("@id", updateReview.ReviewId);
                    updateReviewCmd.Parameters.AddWithValue("@reviewText", updateReview.ReviewText);
                    updateReviewCmd.ExecuteNonQuery();
                    IsSuccessful = true;
                }
            }
            catch
            {
                IsSuccessful = false;
            }
            return (IsSuccessful, toreturn);
        }

        public bool DeleteReview(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    conn.Open();
                    SqlCommand SelectAll = new SqlCommand($"Delete FROM Reviews WHERE Reviews.ReviewId = @ReviewId", conn);
                    SelectAll.Parameters.AddWithValue("@ReviewId", id);
                    try
                    {
                        var num = SelectAll.ExecuteNonQuery();
                        if (!num.Equals(1)) throw new Exception();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
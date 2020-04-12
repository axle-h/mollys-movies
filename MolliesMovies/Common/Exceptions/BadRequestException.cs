using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation.Results;
using Humanizer;
using MolliesMovies.Common.Validation;

namespace MolliesMovies.Common.Exceptions
{
    /// <summary>
    /// An exception indicating a bad request with validation failures for messaging.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="inner">The inner.</param>
        /// <param name="validationFailures">The validation failures.</param>
        /// <exception cref="ArgumentException">The validation result is valid - validationFailures.</exception>
        public BadRequestException(Exception inner, params ValidationFailure[] validationFailures)
            : base(GetMessage(validationFailures), inner)
        {
            if (!validationFailures.Any())
            {
                throw new ArgumentException("The validation result is valid", nameof(validationFailures));
            }

            Validation = new ValidationResult(validationFailures);
        }

        /// <summary>
        /// Gets the validation.
        /// </summary>
        public ValidationResult Validation { get; }

        /// <summary>
        /// Creates a bad request exception informing that something was invalid.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="inner">The inner.</param>
        /// <returns></returns>
        public static BadRequestException Create<TEntity>(Expression<Func<TEntity, object>> property, string reason, Exception inner = null) =>
            new BadRequestException(inner, new ValidationFailure(property.GetMemberName(), reason));

        /// <summary>
        /// Creates a new bad request exception.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <param name="inner">The inner.</param>
        /// <returns></returns>
        public static BadRequestException Create(string reason, Exception inner = null) =>
            new BadRequestException(inner, new ValidationFailure(string.Empty, reason));

        /// <summary>
        /// Creates a bad request exception informing that something was invalid.
        /// </summary>
        /// <param name="name">The name of the invalid object.</param>
        /// <param name="inner">The inner.</param>
        /// <returns></returns>
        public static BadRequestException Invalid(string name, Exception inner = null) => Create($"Invalid {name.Humanize()}", inner);

        /// <summary>
        /// Creates a bad request exception informing that something was invalid.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="inner">The inner.</param>
        /// <returns></returns>
        public static BadRequestException Invalid<TEntity>(Exception inner = null) => Invalid(GetTypeName<TEntity>(), inner);

        /// <summary>
        /// Creates a bad request exception informing that something was invalid.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="inner">The inner.</param>
        /// <returns></returns>
        public static BadRequestException Invalid<TEntity>(Expression<Func<TEntity, object>> property, string reason, Exception inner = null) =>
            new BadRequestException(inner, new ValidationFailure(property.GetMemberName(), reason));

        private static string GetTypeName<TEntity>() => typeof(TEntity).Name.Humanize();

        private static string GetMessage(IEnumerable<ValidationFailure> validationFailures)
        {
            var tokens = validationFailures.Select(f => string.IsNullOrEmpty(f.PropertyName) ? f.ErrorMessage : $"{f.PropertyName}: {f.ErrorMessage}");
            return string.Join(", ", tokens);
        }
    }
}
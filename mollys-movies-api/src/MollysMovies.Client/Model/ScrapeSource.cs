/*
 * Public Molly's Movies API
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using FileParameter = MollysMovies.Client.Client.FileParameter;
using OpenAPIDateConverter = MollysMovies.Client.Client.OpenAPIDateConverter;

namespace MollysMovies.Client.Model
{
    /// <summary>
    /// ScrapeSource
    /// </summary>
    [DataContract(Name = "ScrapeSource")]
    public partial class ScrapeSource : IEquatable<ScrapeSource>, IValidatableObject
    {

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public ScraperType? Type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrapeSource" /> class.
        /// </summary>
        /// <param name="source">source.</param>
        /// <param name="type">type.</param>
        /// <param name="success">success.</param>
        /// <param name="error">error.</param>
        /// <param name="startDate">startDate.</param>
        /// <param name="endDate">endDate.</param>
        /// <param name="movieCount">movieCount.</param>
        /// <param name="torrentCount">torrentCount.</param>
        public ScrapeSource(string? source = default(string?), ScraperType? type = default(ScraperType?), bool? success = default(bool?), string? error = default(string?), DateTime startDate = default(DateTime), DateTime? endDate = default(DateTime?), int movieCount = default(int), int torrentCount = default(int))
        {
            this.Source = source;
            this.Type = type;
            this.Success = success;
            this.Error = error;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.MovieCount = movieCount;
            this.TorrentCount = torrentCount;
        }

        /// <summary>
        /// Gets or Sets Source
        /// </summary>
        [DataMember(Name = "source", EmitDefaultValue = true)]
        public string? Source { get; set; }

        /// <summary>
        /// Gets or Sets Success
        /// </summary>
        [DataMember(Name = "success", EmitDefaultValue = true)]
        public bool? Success { get; set; }

        /// <summary>
        /// Gets or Sets Error
        /// </summary>
        [DataMember(Name = "error", EmitDefaultValue = true)]
        public string? Error { get; set; }

        /// <summary>
        /// Gets or Sets StartDate
        /// </summary>
        [DataMember(Name = "startDate", EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or Sets EndDate
        /// </summary>
        [DataMember(Name = "endDate", EmitDefaultValue = true)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or Sets MovieCount
        /// </summary>
        [DataMember(Name = "movieCount", EmitDefaultValue = false)]
        public int MovieCount { get; set; }

        /// <summary>
        /// Gets or Sets TorrentCount
        /// </summary>
        [DataMember(Name = "torrentCount", EmitDefaultValue = false)]
        public int TorrentCount { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ScrapeSource {\n");
            sb.Append("  Source: ").Append(Source).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Success: ").Append(Success).Append("\n");
            sb.Append("  Error: ").Append(Error).Append("\n");
            sb.Append("  StartDate: ").Append(StartDate).Append("\n");
            sb.Append("  EndDate: ").Append(EndDate).Append("\n");
            sb.Append("  MovieCount: ").Append(MovieCount).Append("\n");
            sb.Append("  TorrentCount: ").Append(TorrentCount).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as ScrapeSource);
        }

        /// <summary>
        /// Returns true if ScrapeSource instances are equal
        /// </summary>
        /// <param name="input">Instance of ScrapeSource to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ScrapeSource input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Source == input.Source ||
                    (this.Source != null &&
                    this.Source.Equals(input.Source))
                ) && 
                (
                    this.Type == input.Type ||
                    this.Type.Equals(input.Type)
                ) && 
                (
                    this.Success == input.Success ||
                    (this.Success != null &&
                    this.Success.Equals(input.Success))
                ) && 
                (
                    this.Error == input.Error ||
                    (this.Error != null &&
                    this.Error.Equals(input.Error))
                ) && 
                (
                    this.StartDate == input.StartDate ||
                    (this.StartDate != null &&
                    this.StartDate.Equals(input.StartDate))
                ) && 
                (
                    this.EndDate == input.EndDate ||
                    (this.EndDate != null &&
                    this.EndDate.Equals(input.EndDate))
                ) && 
                (
                    this.MovieCount == input.MovieCount ||
                    this.MovieCount.Equals(input.MovieCount)
                ) && 
                (
                    this.TorrentCount == input.TorrentCount ||
                    this.TorrentCount.Equals(input.TorrentCount)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Source != null)
                {
                    hashCode = (hashCode * 59) + this.Source.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.Type.GetHashCode();
                if (this.Success != null)
                {
                    hashCode = (hashCode * 59) + this.Success.GetHashCode();
                }
                if (this.Error != null)
                {
                    hashCode = (hashCode * 59) + this.Error.GetHashCode();
                }
                if (this.StartDate != null)
                {
                    hashCode = (hashCode * 59) + this.StartDate.GetHashCode();
                }
                if (this.EndDate != null)
                {
                    hashCode = (hashCode * 59) + this.EndDate.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.MovieCount.GetHashCode();
                hashCode = (hashCode * 59) + this.TorrentCount.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}

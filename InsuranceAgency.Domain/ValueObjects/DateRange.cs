using System;

namespace InsuranceAgency.Domain.ValueObjects
{
    public sealed class DateRange : IEquatable<DateRange>
    {
        public DateOnly Start { get; }
        public DateOnly End { get; }

        public DateRange(DateOnly start, DateOnly end)
        {
            if (end < start) throw new ArgumentException("End must be >= Start");
            Start = start;
            End = end;
        }

        public bool Contains(DateOnly date) => date >= Start && date <= End;

        public override bool Equals(object? obj) => Equals(obj as DateRange);
        public bool Equals(DateRange? other) => other is not null && Start == other.Start && End == other.End;
        public override int GetHashCode() => HashCode.Combine(Start, End);
    }
}

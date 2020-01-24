namespace MusCat.Infrastructure.Validators
{
    static class Be
    {
        public static bool NotEmpty(string name) => !string.IsNullOrWhiteSpace(name);
    }
}

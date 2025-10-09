import random
import string


def generate_random_aliases(base_email: str, count: int, length: int = 8):
    """
    Generate Gmail-style alias emails with random suffixes.
    Example: xhantimzozoyana123+f3k9a8bz@gmail.com
    """
    if "@" not in base_email:
        raise ValueError("Invalid email format")

    name, domain = base_email.split("@")
    aliases = []

    for _ in range(count):
        # Generate random string of letters+digits
        suffix = ''.join(random.choices(string.ascii_lowercase + string.digits, k=length))
        alias = f"{name}+{suffix}@{domain}"
        aliases.append(alias)

    return aliases


if __name__ == "__main__":
    base_email = "xhantimzozoyana123@gmail.com"  # Your email
    alias_count = 20  # Number of aliases you want
    alias_length = 8  # Length of random part

    aliases = generate_random_aliases(base_email, alias_count, alias_length)

    print("Generated aliases:")
    for alias in aliases:
        print(alias)

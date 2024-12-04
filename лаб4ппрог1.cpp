#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>
#include <windows.h>

int main() {
    SetConsoleCP(1251);
    SetConsoleOutputCP(1251);

    char sentence[256];
    printf("Введіть речення: ");

    // Введення рядка з перевіркою на коректність
    if (fgets(sentence, sizeof(sentence), stdin) == NULL) {
        printf("Помилка вводу.\n");
        return 1;
    }

    // Видалення символу нового рядка, якщо він є
    size_t len = strlen(sentence);
    if (len > 0 && sentence[len - 1] == '\n') {
        sentence[len - 1] = '\0';
    }

    // Пошук першого та останнього слова
    char* firstWordStart = sentence;
    char* firstWordEnd = strchr(sentence, ' ');
    if (firstWordEnd == NULL) {
        printf("Речення має містити більше одного слова.\n");
        return 1;
    }

    char* lastWordEnd = sentence + len - 1;
    while (lastWordEnd > sentence && *lastWordEnd == ' ') {
        lastWordEnd--;
    }
    char* lastWordStart = lastWordEnd;
    while (lastWordStart > sentence && *(lastWordStart - 1) != ' ') {
        lastWordStart--;
    }

    // Обмін місцями першого і останнього слова
    int firstWordLength = firstWordEnd - firstWordStart;
    int lastWordLength = lastWordEnd - lastWordStart + 1;

    if (firstWordLength != lastWordLength) {
        printf("Поміняти місцями слова не можна без використання додаткового рядка.\n");
        return 1;
    }

    for (int i = 0; i < firstWordLength; i++) {
        char temp = firstWordStart[i];
        firstWordStart[i] = lastWordStart[i];
        lastWordStart[i] = temp;
    }

    printf("Модифіковане речення: %s\n", sentence);

    return 0;
}


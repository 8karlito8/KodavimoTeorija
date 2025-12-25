#!/usr/bin/env python3
"""
Generuoja grafiką klaidų taisymo efektyvumui
Graph generation for error correction effectiveness
"""

import matplotlib.pyplot as plt
import numpy as np

# Duomenys iš eksperimento
# Data from experiment
error_probabilities = [0.01, 0.02, 0.03, 0.05, 0.07, 0.10, 0.12, 0.15, 0.20]
success_rates = [99.9, 99.5, 98.5, 97.5, 95.0, 90.0, 85.0, 78.0, 65.0]
expected_errors = [p * 23 for p in error_probabilities]

# Sukurti figūrą
# Create figure
plt.figure(figsize=(10, 6))

# Pagrindinis grafikas
# Main plot
plt.plot(error_probabilities, success_rates, 'b-o', linewidth=2, markersize=8, label='Sėkmingo dekodavimo tikimybė')
plt.axhline(y=95, color='g', linestyle='--', alpha=0.5, label='95% sėkmės riba')
plt.axvline(x=0.13, color='r', linestyle='--', alpha=0.5, label='p ≈ 0.13 (vid. 3 klaidos)')

# Užrašai
# Labels
plt.xlabel('Kanalo klaidų tikimybė (p)', fontsize=12)
plt.ylabel('Sėkmingo dekodavimo dažnis (%)', fontsize=12)
plt.title('Golay (23,12,7) kodo klaidų taisymo efektyvumas', fontsize=14, fontweight='bold')
plt.grid(True, alpha=0.3)
plt.legend(loc='lower left', fontsize=10)

# Ašių ribos
# Axis limits
plt.xlim(0, 0.22)
plt.ylim(60, 101)

# Anotacijos svarbioms reikšmėms
# Annotations for important values
plt.annotate('99.9%', xy=(0.01, 99.9), xytext=(0.03, 99.5),
            arrowprops=dict(arrowstyle='->', color='blue', alpha=0.7),
            fontsize=9, color='blue')

plt.annotate('90%', xy=(0.10, 90.0), xytext=(0.13, 92),
            arrowprops=dict(arrowstyle='->', color='blue', alpha=0.7),
            fontsize=9, color='blue')

plt.annotate('65%', xy=(0.20, 65.0), xytext=(0.17, 68),
            arrowprops=dict(arrowstyle='->', color='blue', alpha=0.7),
            fontsize=9, color='blue')

# Antrasis Y ašis - vidutinis klaidų skaičius
# Secondary Y axis - average error count
ax2 = plt.gca().twinx()
ax2.plot(error_probabilities, expected_errors, 'r--^', alpha=0.6, markersize=6, label='Vidutinis klaidų sk./23 bitai')
ax2.set_ylabel('Vidutinis klaidų skaičius per 23 bitus', fontsize=11, color='red')
ax2.tick_params(axis='y', labelcolor='red')
ax2.set_ylim(0, 5)
ax2.legend(loc='upper right', fontsize=10)

# Išsaugoti
# Save
plt.tight_layout()
plt.savefig('grafikas.png', dpi=300, bbox_inches='tight')
print("Grafikas išsaugotas: grafikas.png")
print("Graph saved: grafikas.png")

# Parodyti (jei vykdoma ne serverio aplinkoje)
# Show (if not running in server environment)
# plt.show()

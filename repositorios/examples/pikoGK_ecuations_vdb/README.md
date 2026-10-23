# PikoGK equation catalogue for VDB work

This directory is the reviewed entry point for equations intended for future
PicoGK/OpenVDB geometry work. It does not certify the engine specifications in
the source notes. Keep source material unchanged and only promote an equation
after its assumptions, units, and numerical checks are recorded here.

## First-pass audit

| Topic | Assessment | Use status |
| --- | --- | --- |
| Oxidizer/fuel ratio, `O/F = m_dot_ox / m_dot_fuel` | Correct definition for a mass ratio. The listed 510/140 kg/s gives 3.642857, not exactly 3.6; both can coexist only as rounded reference values. | Usable with explicit units and rounding. |
| Total mass flow, `m_dot = m_dot_ox + m_dot_fuel` | Correct conservation identity for a two-stream total with no other mass streams. | Usable with the stream boundary stated. |
| Thrust, `F = m_dot * Ve + (Pe - Pa) * Ae` | Correct one-dimensional steady nozzle relation for uniform exit velocity and pressure; it is not a complete model of losses or nonuniform flow. | Usable with assumptions and SI units. |
| Specific impulse, `Isp = Ve / g0` | Not generally correct when the pressure-thrust term is nonzero. Use `Isp = F / (m_dot * g0)`. `Ve/g0` applies when `Ve` is the effective exhaust velocity, or for a pressure-matched ideal nozzle. | Do not use the source form unqualified. |
| Circular nozzle area ratio, `epsilon = Ae/At = (De/Dt)^2` | Correct for circular throat and exit sections. The listed `Dt=0.198 m`, `De_SL=0.62 m`, and `De_VAC=1.5 m` imply ratios about 9.805 and 57.392, not 34.34 and 80. | Use the equation; reconcile the dimensions before using the data. |
| Generic cubic bell profile, `R(x)=a0+a1*x+a2*x^2+a3*x^3` | A polynomial form, not a complete nozzle design. Coefficients, boundary conditions, units, and contour constraints are absent. | Not ready to generate geometry. |
| Helix, `r(t)=R*cos(t)`, `z(t)=pitch*t` | Incomplete as a 3D curve because the y coordinate is missing. If `pitch` means axial advance per revolution, use `(R*cos(t), R*sin(t), pitch*t/(2*pi))`. | Specify whether pitch is per radian or per revolution. |
| Gyroid implicit field used as an SDF | The trigonometric expression is an implicit level set, not generally a signed-distance function. A spatially varying frequency also needs a deliberate coordinate mapping and validation. | Do not use as a metric SDF without reinitialization or distance validation. |

## Source notes reviewed

- [Classic injector test notes](../notes/rocket_tests_classic/match/tests.txt)
  and [test data](../notes/rocket_tests_classic/match/notas_o.txt) contain
  experimental tabular values, not governing equations. The OCR has ambiguous
  digits and units and should be checked against the scanned source before
  quantitative use.
- [English engine notes](../kilo_code_instructions_important/ecuations/rocket_pieces/multiple_files/engineInstrucciones.txt)
  and [Spanish engine notes](../kilo_code_instructions_important/ecuations/rocket_pieces/multiple_files/enginespecins.txt)
  repeat equations and parameters but are not independent sources. Their
  `Isp`, area-ratio, bell-profile, and helix claims need the qualifications
  above.
- [Detailed Raptor parameter notes](../kilo_code_instructions_important/ecuations/rocket_pieces/multiple_files/gemini-code-1788533408855.txt)
  give a different throat/exit geometry from the engine notes. The notes also
  mix approximate thrust, mass-flow, and Isp values; treat them as unverified
  inputs, not a consistent design point.
- [Existing AMReX design-case validator](../../nanodb/rocket_advanced/include/DesignCase.h)
  checks that the declared expansion ratio agrees with the diameters. It does
  not validate combustion, thrust, or heat-transfer models.

## Promotion criteria

For every equation added here, record its source, variable definitions, SI
units, applicability assumptions, limiting cases, and a numerical test. Keep
empirical reference values separate from identities derived from conservation
laws. Thermal, material, combustion, and turbomachinery equations in the source
files have not yet been audited individually and are not approved for use.
#include <cassert>
#include <cmath>
#include <string>

#include "DesignCase.h"

namespace {

bool nearlyEqual(double a, double b, double tol) {
    return std::fabs(a - b) <= tol;
}

} // namespace

int main() {
    rocket_advanced::DesignCase design_case;
    design_case.case_id = "lox_ch4_concept_reference";
    design_case.thermodynamics.chamber_pressure_pa = 30.0e6;
    design_case.thermodynamics.combustion_temperature_k = 3500.0;
    design_case.thermodynamics.mixture_ratio_oxidizer_to_fuel = 3.6;
    design_case.thermodynamics.characteristic_velocity_m_per_s = 1700.0;
    design_case.geometry.chamber_count = 4;
    design_case.geometry.chamber_diameter_m = 0.38;
    design_case.geometry.chamber_length_m = 1.1;
    design_case.geometry.throat_diameter_m = 0.13;
    design_case.geometry.exit_diameter_m = 1.1;
    design_case.geometry.nozzle_length_m = 3.0;
    design_case.geometry.expansion_ratio = (1.1 / 0.13) * (1.1 / 0.13);
    design_case.turbomachinery.shaft_speed_rpm = 28000.0;
    design_case.turbomachinery.lox_mass_flow_kg_per_s = 900.0;
    design_case.turbomachinery.methane_mass_flow_kg_per_s = 380.0;
    design_case.turbomachinery.pump_power_w = 140.0e6;

    std::string error;
    assert(rocket_advanced::validateDesignCase(design_case, error));

    const double mixture_ratio = design_case.turbomachinery.lox_mass_flow_kg_per_s /
                                design_case.turbomachinery.methane_mass_flow_kg_per_s;
    assert(nearlyEqual(mixture_ratio, 2.36842105263, 1e-9));

    const double throat_area = M_PI * std::pow(design_case.geometry.throat_diameter_m * 0.5, 2.0);
    const double exit_area = M_PI * std::pow(design_case.geometry.exit_diameter_m * 0.5, 2.0);
    const double area_ratio = exit_area / throat_area;
    assert(nearlyEqual(area_ratio, design_case.geometry.expansion_ratio, 1e-9));

    design_case.geometry.exit_diameter_m = 0.10;
    assert(!rocket_advanced::validateDesignCase(design_case, error));

    rocket_advanced::DesignCase reduced_case = design_case;
    reduced_case.geometry.exit_diameter_m = 1.1;
    reduced_case.turbomachinery.lox_mass_flow_kg_per_s = 510.0;
    reduced_case.turbomachinery.methane_mass_flow_kg_per_s = 140.0;
    const double reference_mixture_ratio =
        reduced_case.turbomachinery.lox_mass_flow_kg_per_s /
        reduced_case.turbomachinery.methane_mass_flow_kg_per_s;
    assert(std::fabs(reference_mixture_ratio - 3.642857142857) < 1e-6);

    reduced_case.geometry.throat_diameter_m = 0.198;
    reduced_case.geometry.exit_diameter_m = 0.62;
    const double geometric_area_ratio =
        (0.5 * reduced_case.geometry.exit_diameter_m) * (0.5 * reduced_case.geometry.exit_diameter_m) /
        ((0.5 * reduced_case.geometry.throat_diameter_m) * (0.5 * reduced_case.geometry.throat_diameter_m));
    assert(std::fabs(geometric_area_ratio - 9.8) < 5e-2);

    return 0;
}
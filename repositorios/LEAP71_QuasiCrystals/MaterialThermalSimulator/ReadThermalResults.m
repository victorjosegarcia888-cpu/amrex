% MATLAB import instructions for thermal simulator results
data = readtable('Temperatures.csv');
plot(data.node, data.temperature, '-o');
xlabel('Node');
ylabel('Temperature (K)');
title('1D Thermal Simulation Results');
grid on;

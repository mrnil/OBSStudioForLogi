# Test Driven Development

1. All new business logic and core functionality must have accompanying tests
2. Tests should be written before or alongside implementation where practical
3. The following are exempt from strict TDD requirements:
   - Loupedeck Plugin API integration code (Commands, ClientApplication)
   - UI event handlers and display updates
   - Third-party library adapters (when behavior is pass-through)
4. Minimum test coverage targets:
   - Services layer: 80%+ coverage
   - Business logic: 90%+ coverage
   - Actions/Commands: Integration tests for critical paths

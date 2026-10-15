# GitHub Copilot Instructions

## Code Style Guidelines

### Type Declarations
- Use only explicit types, never use `var`. This includes foreach loops, LINQ results, and tuple deconstructions. For example: use `EntityEntry<T> entry` instead of `var entry`, and `(Type1 prop1, Type2 prop2)` instead of `var (prop1, prop2)` in deconstructions.
- Always declare the specific type for variables.

### Variable Organization
- Keep all variable declarations at the start of methods.
- Group related variables together.

### .NET 10.0 Optimization
- Optimize the code with all the functionalities and improvements of .NET 10.0.
- Leverage modern .NET features where applicable.

### Communication
- Maintain answers as short as possible.
- Be concise and direct.

### Generic Solutions
- Never create a solution that works with only one case.
- Always create something generic and reusable.

### Constants vs Hardcoding
- If you must write something, create a constant.
- Don't do anything hardcoded.
- Define constants at appropriate scope (class level or method level).

### Testing and Verification
- Always try if the solution you created for a request is working.
- Reduce the output as much as possible.
- If necessary, create a test case to check functionality.
- Run the program and read the output to see if everything is correct.
- In case of negative results, repeat the operation until the output is what is requested.
- After you found a working solution, safely remove the test case.

### Performance and Safety
- Make all changes in the fastest but secure way possible.
- Every time you analyze the code it takes time, so try to do everything in the least number of steps possible but without breaking the code.
- If necessary, create a backup of the code before making changes.

### Code Comments
- Don't remove any comment if they are still valid.
- Preserve existing documentation and inline comments.

### Task Completion
- Every time you completely finish a task, your last message must be: "Task finished: good to go"

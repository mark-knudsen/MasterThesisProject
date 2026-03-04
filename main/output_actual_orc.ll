; ModuleID = 'repl_module'
source_filename = "repl_module"

define i64 @__anon_expr_1() {
entry:
  %arr_ptr = call ptr @malloc(i64 24)
  %idx_0 = getelementptr i64, ptr %arr_ptr, i32 0
  store i64 4607182418800017408, ptr %idx_0, align 4
  %idx_1 = getelementptr i64, ptr %arr_ptr, i32 1
  store i64 4611686018427387904, ptr %idx_1, align 4
  %idx_2 = getelementptr i64, ptr %arr_ptr, i32 2
  store i64 4613937818241073152, ptr %idx_2, align 4
  %ptr_to_i32 = ptrtoint ptr %arr_ptr to i32
  ret i32 %ptr_to_i32
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

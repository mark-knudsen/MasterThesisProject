; ModuleID = 'repl_module'
source_filename = "repl_module"

@true_str = private unnamed_addr constant [6 x i8] c"true\0A\00", align 1
@false_str = private unnamed_addr constant [7 x i8] c"false\0A\00", align 1
@x = global ptr null, align 8

define i64 @exec_205dc8702f1c4c21819677e9594f30a1() {
entry:
  %arr_ptr = call ptr @malloc(i64 24)
  %idx_0 = getelementptr i64, ptr %arr_ptr, i32 0
  store i64 4607182418800017408, ptr %idx_0, align 4
  %idx_1 = getelementptr i64, ptr %arr_ptr, i32 1
  store i64 4611686018427387904, ptr %idx_1, align 4
  %idx_2 = getelementptr i64, ptr %arr_ptr, i32 2
  store i64 4613937818241073152, ptr %idx_2, align 4
  store ptr %arr_ptr, ptr @x, align 8
  %x = load ptr, ptr @x, align 8
  %arr_ptr1 = call ptr @malloc(i64 8)
  %idx_02 = getelementptr i64, ptr %arr_ptr1, i32 0
  store i64 0, ptr %idx_02, align 4
  %ptr_to_i32 = ptrtoint ptr %arr_ptr1 to i32
  ret i32 %ptr_to_i32
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

; ModuleID = 'repl_module'
source_filename = "repl_module"

define { i32, ptr } @main_2() {
entry:
  %randcall = call i32 @rand()
  %rand_result_ptr = alloca i32, align 4
  br i1 false, label %rand.correct, label %rand.swap

rand.correct:                                     ; preds = %entry
  %mod1 = srem i32 %randcall, -4998
  %res1 = add i32 %mod1, 5000
  store i32 %res1, ptr %rand_result_ptr, align 4
  br label %rand.cont

rand.swap:                                        ; preds = %entry
  %mod2 = srem i32 %randcall, 5000
  %res2 = add i32 %mod2, 1
  store i32 %res2, ptr %rand_result_ptr, align 4
  br label %rand.cont

rand.cont:                                        ; preds = %rand.swap, %rand.correct
  %final_rand_int = load i32, ptr %rand_result_ptr, align 4
  %final_rand_dbl = sitofp i32 %final_rand_int to double
  %num_mem = call ptr @malloc(i64 8)
  store double %final_rand_dbl, ptr %num_mem, align 8
  %with_data = insertvalue { i32, ptr } { i32 1, ptr undef }, ptr %num_mem, 1
  ret { i32, ptr } %with_data
}

declare i32 @printf(ptr, ...)

declare i32 @rand()

declare ptr @malloc(i64)

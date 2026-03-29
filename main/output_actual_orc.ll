; ModuleID = 'repl_module'
source_filename = "repl_module"

define ptr @main_4() {
entry:
  %randcall = call i64 @rand()
  %rand_result_ptr = alloca i64, align 8
  br i1 true, label %rand.correct, label %rand.swap

rand.correct:                                     ; preds = %entry
  %mod1 = srem i64 %randcall, 10
  %res1 = add i64 %mod1, 1
  store i64 %res1, ptr %rand_result_ptr, align 4
  br label %rand.cont

rand.swap:                                        ; preds = %entry
  %mod2 = srem i64 %randcall, -8
  %res2 = add i64 %mod2, 10
  store i64 %res2, ptr %rand_result_ptr, align 4
  br label %rand.cont

rand.cont:                                        ; preds = %rand.swap, %rand.correct
  %final_rand_int = load i64, ptr %rand_result_ptr, align 4
  %value_mem = call ptr @malloc(i64 8)
  store i64 %final_rand_int, ptr %value_mem, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare i64 @rand()

declare ptr @malloc(i64)

; ModuleID = 'repl_module'
source_filename = "repl_module"

define ptr @main_8() {
entry:
  %rand = call i64 @rand()
  br i1 true, label %rand.int.ok, label %rand.int.swap

rand.int.ok:                                      ; preds = %entry
  %mod1 = srem i64 %rand, 100
  %res1 = add i64 %mod1, 1
  br label %rand.int.merge

rand.int.swap:                                    ; preds = %entry
  %mod2 = srem i64 %rand, -98
  %res2 = add i64 %mod2, 100
  br label %rand.int.merge

rand.int.merge:                                   ; preds = %rand.int.swap, %rand.int.ok
  %rand_int = phi i64 [ %res1, %rand.int.ok ], [ %res2, %rand.int.swap ]
  %value_mem = call ptr @malloc(i64 8)
  %value_cast = bitcast ptr %value_mem to ptr
  store i64 %rand_int, ptr %value_cast, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare i64 @rand()

declare ptr @malloc(i64)
